using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace RoughtSheetEncore
{
    public partial class Form1 : Form
    {

        #region Declarations
        public enum NewVariationOption
        {
            NotDefined,
            NewVariation,
            NewMainLine,
            Overwrite,
            Cancel
        }
        public enum PlayMode
        {
            SinglePlayer,
            TwoPlayer,
            EngineVsEngine,
            EditPosition
        }

        static readonly object locker = new object();
        NewVariationOption newVarOption;
        PlayMode ModeOfPlay;
        GameDetails gameDetails;
        Move LastMoveHighlighted;
        List<Square> HighlightedSquares;
        Piece SelectedPiece, LastHighlightedPiece;
        Piece.PieceSide sideToPlay;
        static Piece WhiteKing, BlackKing;
        static bool IsClicked, isHighlighted, IsWhiteInCheck, IsBlackInCheck, IsWhiteCheckmated, 
            IsBlackCheckmated, IsDraw, tester, KingsideCastlingWhite, QueensideCastlingWhite,
            KingsideCastlingBlack, QueensideCastlingBlack, isCtrl, isShift, isAlt, ShouldDrawUserArrows;
        int FiftyMoveCount, MoveCount;
        static Piece PieceClicked, CheckingPiece, DoubleCheckingPiece, PromotePiece, PromotePawn, EnPassantPawn, PieceMoving;
        Square PromoteSquare;
        PresetTheme ColorTheme;
        Point PointClicked;
        Form PromoteForm, NewVariationForm, VariationSelectForm;
        Position CurrentPosition;
        Arrow DrawnLine;
        static List<Square> Squares;
        List<VariationHolder> GameVariations;
        List<Square> ValidSquares;
        static List<Piece> Pieces;
        List<Piece> CapturedPieces;
        List<Move> PossibleDefences;
        List<Square> PossibleBlockingSquares;
        VariationHolder MainLine;
        VariationHolder CurrentVariation;
        static Brush LightSquareColor;
        static Brush DarkSquareColor;
        static Bitmap WhitePawnImage, BlackPawnImage, WhiteKnightImage, BlackKnightImage, WhiteBishopImage,
            BlackBishopImage, WhiteRookImage, BlackRookImage, WhiteQueenImage, BlackQueenImage,
            WhiteKingImage, BlackKingImage;
        Position StartingPosition;
        Font EngineWindowFont;
        static Color LegalMoveHLColor, ShiftColor, AltColor, CtrlColor, LastMoveHLColor, BMAColor;
        PanelProfile panelProfile;
        Square[,] TwoDSquares;
        Label Name_EloLabel, Event_DateLabel, OpeningLabel;
        static User CurrentUser;
        List<User> UserList;
        Font gdDisplayFont;
        #endregion

        public Form1()
        {
            InitializeComponent();

            InitializeSettings();
            InitializeState();
            timer1.Enabled = true;
            timer1.Start();

            DoubleBuffered = true;
            InitializeFonts();
            EnginePanel.BackColor = this.BackColor;
            this.KeyUp += Form1_KeyUp;
            HighlightedSquares = new List<Square>();
            InitializeImages();
            this.KeyDown += Form1_KeyDown;
            flowLayoutPanel1.MouseDown += flowLayoutPanel1_MouseDown;       
            flowLayoutPanel1.AutoScroll = false;
            flowLayoutPanel1.HorizontalScroll.Enabled = false;
            flowLayoutPanel1.HorizontalScroll.Visible = false;
            flowLayoutPanel1.AutoScroll = true;

            ScrollSize1 = new Size(flowLayoutPanel1.Width, flowLayoutPanel1.Height);
            ScrollSize2 = new Size(flowLayoutPanel1.Width + 20, flowLayoutPanel1.Height);

            SetUpClocks_Utilities();
            InitializeNotationContextMenu();

            BinaryFormatter bf = new BinaryFormatter();
            using (Stream input = File.OpenRead("OpeningBookModified.dat"))
            {
                TabierList = bf.Deserialize(input) as List<OpeningNode>;
            }

            gdDisplayFont = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Bold);
            Name_EloLabel = new Label();
            Event_DateLabel = new Label();
            OpeningLabel = new Label();
            Name_EloLabel.Font = gdDisplayFont;
            Event_DateLabel.Font = new System.Drawing.Font(gdDisplayFont.OriginalFontName, 
                gdDisplayFont.Size, FontStyle.Regular);
            OpeningLabel.Font = new System.Drawing.Font(gdDisplayFont.OriginalFontName,
                gdDisplayFont.Size - 1, FontStyle.Regular);
            Name_EloLabel.AutoSize = true;
            Event_DateLabel.AutoSize = true;
            OpeningLabel.AutoSize = true;
            tabPage1.Controls.Add(Name_EloLabel);
            tabPage1.Controls.Add(Event_DateLabel);
            tabPage1.Controls.Add(OpeningLabel);

            SetupUCI();
            if (Squares == null)
                InitializeSquares();

            RunTourneyUtilities();
        }
        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (ShouldDrawUserArrows)
            {
                isCtrl = e.Control;
                isAlt = e.Alt;
                isShift = e.Shift; 
            }
        }
        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (ShouldDrawUserArrows)
            {
                isCtrl = e.Control;
                isAlt = e.Alt;
                isShift = e.Shift;

                if ((isCtrl || isAlt || isShift) && settings.appearance.SaveUserArrows == "Save with position")
                {
                    List<Square> list = new List<Square>();
                    foreach (var item in CurrentPosition.Lines)
                        if (!item.Enabled)
                        {
                            item.Enabled = true;
                            list.AddRange(item.Squares);
                        }
                    RefreshSquares(list, RedrawPerspective.Arrow, null);
                }
            }

            if (e.Control && e.KeyCode == Keys.T)
            {
                ShowSettingsForm();
            }
            if (e.Control)
            {
                if (e.KeyCode == Keys.M)
                {
                    DisplayCommentForm();
                }
                else if (e.KeyCode == Keys.F)
                {
                    FlipBoard();
                }
                else if (e.KeyCode == Keys.A)
                {
                    AnalyzeButton_Click(FirstEngine.AnalyzeButton, new EventArgs());
                }
            }
            if (e.KeyCode == Keys.Right)
            {
                if (IsAnimating)
                    return;
                LoadNextPosition();
                if (focusLabel != null)
                    flowLayoutPanel1.ScrollControlIntoView(focusLabel);
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (IsAnimating)
                    return;
                LoadPreviousPosition();
                if (focusLabel != null)
                    flowLayoutPanel1.ScrollControlIntoView(focusLabel);
            }
            else if (e.KeyCode == Keys.Home)
            {
                if (IsAnimating)
                    return;
                CurrentVariation = MainLine;
                LoadPosition(MainLine.MovesList[0], false);
                if (focusLabel != null)
                {
                    focusLabel.Font = NotationFont;
                    if (focusLabel.Tag == MainLine)
                        focusLabel.ForeColor = NotationMainForeColor;
                    else
                        focusLabel.ForeColor = NotationSubForeColor;
                    focusLabel = null;
                    flowLayoutPanel1.ScrollControlIntoView(flowLayoutPanel1.Controls[0]);
                }
            }
            else if (e.KeyCode == Keys.End)
            {
                if (IsAnimating)
                    return;
                LoadPosition(CurrentVariation.MovesList[CurrentVariation.MovesList.Count - 1], false);
                Label LastVarLabel = new Label();
                foreach (var itemx in flowLayoutPanel1.Controls)
                {
                    Label item = itemx as Label;
                    if ((VariationHolder)item.Tag == CurrentVariation && item.Name.Length > 1)
                        LastVarLabel = item;
                }
                if (focusLabel != null)
                {
                    focusLabel.Font = NotationFont;
                    focusLabel.ForeColor = NotationMainForeColor;
                }
                focusLabel = LastVarLabel;
                LastVarLabel.Font = NotationFont;
                LastVarLabel.ForeColor = NotationFocusColor;
                flowLayoutPanel1.ScrollControlIntoView(focusLabel);
            }

            else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Up)
            {
                int x = CurrentVariation.MovesList.IndexOf(CurrentPosition);
                if (x <= 10)
                    LoadPosition(CurrentVariation.MovesList[0], false);
                else
                    LoadPosition(CurrentVariation.MovesList[x - 10], false);

                UpdateFocusLabel();
            }

            else if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Down)
            {
                int x = CurrentVariation.MovesList.IndexOf(CurrentPosition);
                if (CurrentVariation.MovesList.Count <= x + 10)
                    LoadPosition(CurrentVariation.MovesList
                        [CurrentVariation.MovesList.Count - 1], false);
                else
                    LoadPosition(CurrentVariation.MovesList[x + 10], false);

                UpdateFocusLabel();
            }
        }
        private void InitializeFonts()
        {
            if (EngineWindowFont == null)
                EngineWindowFont = new Font("Segoe UI", 11F, FontStyle.Bold);
            NameButton1.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            DepthLabel1.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Regular);
            EvaluationLabel1.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            AnalyzeButton1.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            GoButton1.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            PVLabel1.Font =
                new Font(EngineWindowFont.OriginalFontName, 9F, FontStyle.Regular);
            NameButton2.Font =
                 new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            DepthLabel2.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Regular);
            EvaluationLabel2.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            AnalyzeButton2.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            GoButton2.Font =
                new Font(EngineWindowFont.OriginalFontName, 11F, FontStyle.Bold);
            PVLabel2.Font =
                new Font(EngineWindowFont.OriginalFontName, 9F, FontStyle.Regular);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right
                || keyData == Keys.Up || keyData == Keys.Down)
            {
                Form1_KeyDown(this, new KeyEventArgs(keyData));
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void StartNewGame(bool isPlayOut)
        {
            if (StartingPosition != null)       // Test this, abeg
            {
                LoadPosition(StartingPosition, isPlayOut);
                CapturedPieces = new List<Piece>();
                GameVariations = new List<VariationHolder>();
                MainLine = new VariationHolder();
                GameVariations.Add(MainLine);
                MainLine.MovesList = new List<Position>();
                MainLine.MovesList.Add(StartingPosition);
                CurrentPosition = StartingPosition;
                CurrentVariation = MainLine;

                flowLayoutPanel1.Controls.Clear();
                flowLayoutPanel1.BringToFront();

                if (LoadedEngines != null)
                    foreach (var item in LoadedEngines)
                        item.Process.StandardInput.WriteLine("ucinewgame");

                return;
            }
            sideToPlay = Piece.PieceSide.White;
            FiftyMoveCount = 0;
            MoveCount = 0;
            KingsideCastlingWhite = true;
            QueensideCastlingWhite = true;
            KingsideCastlingBlack = true;
            QueensideCastlingBlack = true;
            IsWhiteCheckmated = false;
            IsBlackCheckmated = false;
            IsDraw = false;
            GameVariations = new List<VariationHolder>();

            if(!isPlayOut)
                InitializePieces();
            MainLine = new VariationHolder();
            GameVariations.Add(MainLine);
            MainLine.MovesList = new List<Position>();
            Position TempPosition = new Position();
            TempPosition.CheckingPiece = CheckingPiece;
            newVarOption = NewVariationOption.NotDefined;
            TempPosition.DoubleCheckingPiece = DoubleCheckingPiece;
            TempPosition.IsBlackCheckmated = IsBlackCheckmated;
            TempPosition.IsBlackInCheck = IsBlackInCheck;
            TempPosition.IsDraw = IsDraw;
            TempPosition.IsWhiteCheckmated = IsWhiteCheckmated;
            TempPosition.IsWhiteInCheck = IsWhiteInCheck;
            TempPosition.PossibleDefences = PossibleDefences;
            TempPosition.EnPassantPawn = EnPassantPawn;
            TempPosition.KingsideCastlingBlack = KingsideCastlingBlack;
            TempPosition.KingsideCastlingWhite = KingsideCastlingWhite;
            TempPosition.PieceInfos = ClonePieces();
            TempPosition.QueensideCastlingBlack = QueensideCastlingBlack;
            TempPosition.QueensideCastlingWhite = QueensideCastlingWhite;
            TempPosition.sideToPlay = sideToPlay;
            TempPosition.FiftyMoveCount = FiftyMoveCount;
            TempPosition.MoveCount = MoveCount;
            MainLine.MovesList.Add(TempPosition);
            if (StartingPosition == null)
                StartingPosition = TempPosition;
            CurrentPosition = TempPosition;
            CurrentVariation = MainLine;

            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.BringToFront();

            FENforStartPos = "";
            if (LoadedEngines != null)
                foreach (var item in LoadedEngines)
                    item.Process.StandardInput.WriteLine("ucinewgame");
        }
        private void InitializeImages()
        {
            // Original -> 456
            if (panelProfile == null)
                panelProfile = new PanelProfile(ChessFont.Merida);
            int height = (int)(panel1.Height * panelProfile.PawnSize.Item1),
                width = (int)(height * panelProfile.PawnSize.Item2);
            Bitmap tempImage = new Bitmap(Properties.Resources.pawn, width, height);
            WhitePawnImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            tempImage = new Bitmap(Properties.Resources.b_pawn, width, height);
            BlackPawnImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            //32bppPArgb
            height = (int)(panel1.Height * panelProfile.KnightSize.Item1);
                width = (int)(height * panelProfile.KnightSize.Item2);
            tempImage = new Bitmap(Properties.Resources.knight, width, height);
            WhiteKnightImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            tempImage = new Bitmap(Properties.Resources.b_knight, width, height);
            BlackKnightImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            height = (int)(panel1.Height * panelProfile.BishopSize.Item1);
            width = (int)(height * panelProfile.BishopSize.Item2);
            tempImage = new Bitmap(Properties.Resources.bishop, width, height);
            WhiteBishopImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            tempImage = new Bitmap(Properties.Resources.b_bishop, width, height);
            BlackBishopImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            height = (int)(panel1.Height * panelProfile.RookSize.Item1);
            width = (int)(height * panelProfile.RookSize.Item2);
            tempImage = new Bitmap(Properties.Resources.rook, width, height);
            WhiteRookImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            tempImage = new Bitmap(Properties.Resources.b_rook, width, height);
            BlackRookImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            height = (int)(panel1.Height * panelProfile.QueenSize.Item1);
            width = (int)(height * panelProfile.QueenSize.Item2);
            tempImage = new Bitmap(Properties.Resources.queen, width, height);
            WhiteQueenImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            tempImage = new Bitmap(Properties.Resources.b_queen, width, height);
            BlackQueenImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            
            height = (int)(panel1.Height * panelProfile.KingSize.Item1);
            width = (int)(height * panelProfile.KingSize.Item2);
            tempImage = new Bitmap(Properties.Resources.king, width, height);
            WhiteKingImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            tempImage = new Bitmap(Properties.Resources.b_king, width, height);
            BlackKingImage = tempImage.Clone(new Rectangle(0, 0, width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        }
        private void InitializeSquares()
        {
            Point LocPoint = new Point();
            Squares = new List<Square>();
            for (int i = 0; i < 64; i++)
            {
                Square tempSquare = new Square();
                int a = panel1.Height / 8;
                switch (i / 8)
                {
                    case (0): tempSquare.Name = "a"; LocPoint.X = 0; break;
                    case (1): tempSquare.Name = "b"; LocPoint.X = a; break;
                    case (2): tempSquare.Name = "c"; LocPoint.X = 2 * a; break;
                    case (3): tempSquare.Name = "d"; LocPoint.X = 3 * a; break;
                    case (4): tempSquare.Name = "e"; LocPoint.X = 4 * a; break;
                    case (5): tempSquare.Name = "f"; LocPoint.X = 5 * a; break;
                    case (6): tempSquare.Name = "g"; LocPoint.X = 6 * a; break;
                    case (7): tempSquare.Name = "h"; LocPoint.X = 7 * a; break;
                }
                switch (i % 8)
                {
                    case (0): tempSquare.Name += "1"; LocPoint.Y = a * 7; break;
                    case (1): tempSquare.Name += "2"; LocPoint.Y = a * 6; break;
                    case (2): tempSquare.Name += "3"; LocPoint.Y = a * 5; break;
                    case (3): tempSquare.Name += "4"; LocPoint.Y = a * 4; break;
                    case (4): tempSquare.Name += "5"; LocPoint.Y = a * 3; break;
                    case (5): tempSquare.Name += "6"; LocPoint.Y = a * 2; break;
                    case (6): tempSquare.Name += "7"; LocPoint.Y = a; break;
                    case (7): tempSquare.Name += "8"; LocPoint.Y = 0; break;
                }
                tempSquare.Rectangle = new Rectangle(LocPoint, new Size(a, a));
                Squares.Add(tempSquare);
            }
            using (Graphics g = panel1.CreateGraphics())
            {
                bool isDark = true;
                int x = 1;
                foreach (Square item in Squares)
                {
                    if (isDark)
                    {
                        g.FillRectangle((DarkSquareColor), item.Rectangle);
                        item.Type = Square.SquareType.Dark;
                        isDark = false;
                    }
                    else
                    {
                        g.FillRectangle((LightSquareColor), item.Rectangle);
                        item.Type = Square.SquareType.Light;
                        isDark = true;
                    }
                    if (x % 8 == 0)
                    {
                        isDark = !isDark;
                    }
                    x++;

                    String rank = (9 - int.Parse(item.Name.Substring(1, 1))).ToString();
                    char file = item.Name[0];
                    int b = 7 - (file - 'a');
                    file = 'a';
                    file += (char)b;
                    item.FlipSquare = GetSquare(file + rank);
                }
            }
            if (TwoDSquares == null)
            {
                TwoDSquares = new Square[8, 8];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        TwoDSquares[i, j] = Squares[i + j * 8];
                    }
                }
            }

            //For Position Edit panel
            EDPanel.Size = new System.Drawing.Size(400, 400);
            int index = 0, z = EDPanel.Height / 8;
            Square CurrentSquare;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++, index++)
                {
                    CurrentSquare = Squares[index];
                    CurrentSquare.EDRectangle = new Rectangle(new Point(i * z, z * (7 - j)), new Size(z, z));
                }
            }

            InitializeSquareCordinates();
        }
        private void InitializeSquareCordinates()
        {
            Graphics g = gamePanel.CreateGraphics();
            g.Clear(gamePanel.BackColor);
            g.DrawRectangle(Pens.Black, new Rectangle(new Point(panel1.Left - 1, panel1.Top - 1),
                panel1.Size + new Size(1, 1)));
            gamePanel.Size = panel1.Size + new Size(30, 30);
            gamePanel.Location = new Point(5, 50);
            panel1.Location = new Point(15, 15);

            if (settings.appearance.BoardCordinatesSides == "None")
                return;
            char tempChar = 'a', tempChar2 = 'A';
            Font SquareCordFont = new Font("Calibri", 8F, FontStyle.Regular);
            for (int i = 0; i < 8; i++, tempChar++, tempChar2++)
            {
                Square square = GetSquare((tempChar).ToString() + "1");
                int midX = (square.Rectangle.Right - square.Rectangle.Left) / 2 
                    + square.Rectangle.Left;
                g.DrawString((tempChar2).ToString(), SquareCordFont,
                    Brushes.Black, new PointF(midX + 13, 3));
                if (settings.appearance.BoardCordinatesSides == "All sides")
                    g.DrawString((tempChar2).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(midX + 13, gamePanel.Height - 15));
            }
            if (isBoardFlipped)
            {
                tempChar = 'a';
                for (int i = 8; i > 0; i--)
                {
                    Square square = GetSquare((tempChar).ToString() + i);
                    int midY = (square.Rectangle.Bottom - square.Rectangle.Top) / 2
                        + square.Rectangle.Top;
                    g.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(3, midY + 13));
                    if (settings.appearance.BoardCordinatesSides == "All sides")
                        g.DrawString((i).ToString(), SquareCordFont,
                            Brushes.Black, new PointF(gamePanel.Right - 15, midY + 13));
                }
            }
            else
            {
                tempChar = 'a';
                for (int i = 1; i < 9; i++)
                {
                    Square square = GetSquare((tempChar).ToString() + i);
                    int midY = (square.Rectangle.Bottom - square.Rectangle.Top) / 2
                        + square.Rectangle.Top;
                    g.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(3, midY + 13));
                    if (settings.appearance.BoardCordinatesSides == "All sides")
                    g.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(gamePanel.Right - 15, midY + 13));
                } 
            }
        }
        private void InitializePieces()
        {
            InitializePieces("");
        }
        private void InitializePieces(String EDIT)
        {
            List<Piece> _Pieces = new List<Piece>();
            if (EDIT == "")
                CapturedPieces = new List<Piece>();
            Piece tempPiece;
            for (int i = 0; i < 8; i++)
            {
                tempPiece = new Piece();
                tempPiece.Name = "White Pawn";
                tempPiece.Side = Piece.PieceSide.White;
                tempPiece.Type = Piece.PieceType.Pawn;
                tempPiece.Image = WhitePawnImage;
                _Pieces.Add(tempPiece);
            }
            PlacePiece(_Pieces[0], GetSquare("a2"), EDIT); PlacePiece(_Pieces[1], GetSquare("b2"), EDIT);
            PlacePiece(_Pieces[2], GetSquare("c2"), EDIT); PlacePiece(_Pieces[3], GetSquare("d2"), EDIT);
            PlacePiece(_Pieces[4], GetSquare("e2"), EDIT); PlacePiece(_Pieces[5], GetSquare("f2"), EDIT);
            PlacePiece(_Pieces[6], GetSquare("g2"), EDIT); PlacePiece(_Pieces[7], GetSquare("h2"), EDIT);

            for (int i = 8; i < 16; i++)
            {
                tempPiece = new Piece("Black Pawn", Piece.PieceType.Pawn,
                    Piece.PieceSide.Black, BlackPawnImage);
                _Pieces.Add(tempPiece);
            }
            PlacePiece(_Pieces[8], GetSquare("a7"), EDIT); PlacePiece(_Pieces[9], GetSquare("b7"), EDIT);
            PlacePiece(_Pieces[10], GetSquare("c7"), EDIT); PlacePiece(_Pieces[11], GetSquare("d7"), EDIT);
            PlacePiece(_Pieces[12], GetSquare("e7"), EDIT); PlacePiece(_Pieces[13], GetSquare("f7"), EDIT);
            PlacePiece(_Pieces[14], GetSquare("g7"), EDIT); PlacePiece(_Pieces[15], GetSquare("h7"), EDIT);

            tempPiece = new Piece("White Knight", Piece.PieceType.Knight,
                Piece.PieceSide.White, WhiteKnightImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("b1"), EDIT);
            tempPiece = new Piece("White Knight", Piece.PieceType.Knight,
                Piece.PieceSide.White, WhiteKnightImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("g1"), EDIT);
            tempPiece = new Piece("White Bishop", Piece.PieceType.Bishop,
                Piece.PieceSide.White, WhiteBishopImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("c1"), EDIT);
            tempPiece = new Piece("White Bishop", Piece.PieceType.Bishop,
                Piece.PieceSide.White, WhiteBishopImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("f1"), EDIT);
            tempPiece = new Piece("White Rook", Piece.PieceType.Rook,
                Piece.PieceSide.White, WhiteRookImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("a1"), EDIT);
            tempPiece = new Piece("White Rook", Piece.PieceType.Rook,
                Piece.PieceSide.White, WhiteRookImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("h1"), EDIT);
            tempPiece = new Piece("White Queen", Piece.PieceType.Queen,
                Piece.PieceSide.White, WhiteQueenImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("d1"), EDIT);
            tempPiece = new Piece("White King", Piece.PieceType.King,
                Piece.PieceSide.White, WhiteKingImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("e1"), EDIT);

            tempPiece = new Piece("Black Knight", Piece.PieceType.Knight,
                Piece.PieceSide.Black, BlackKnightImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("b8"), EDIT);
            tempPiece = new Piece("Black Knight", Piece.PieceType.Knight,
                Piece.PieceSide.Black, BlackKnightImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("g8"), EDIT);
            tempPiece = new Piece("Black Bishop", Piece.PieceType.Bishop,
                Piece.PieceSide.Black, BlackBishopImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("c8"), EDIT);
            tempPiece = new Piece("Black Bishop", Piece.PieceType.Bishop,
                Piece.PieceSide.Black, BlackBishopImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("f8"), EDIT);
            tempPiece = new Piece("Black Rook", Piece.PieceType.Rook,
                Piece.PieceSide.Black, BlackRookImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("a8"), EDIT);
            tempPiece = new Piece("Black Rook", Piece.PieceType.Rook,
                Piece.PieceSide.Black, BlackRookImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("h8"), EDIT);
            tempPiece = new Piece("Black Queen", Piece.PieceType.Queen,
                Piece.PieceSide.Black, BlackQueenImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("d8"), EDIT);
            tempPiece = new Piece("Black King", Piece.PieceType.King,
                Piece.PieceSide.Black, BlackKingImage);
            _Pieces.Add(tempPiece); PlacePiece(tempPiece, GetSquare("e8"), EDIT);

            if (EDIT == "")
            {
                Pieces = _Pieces;
                foreach (Piece p in _Pieces)
                {
                    if (p.Type == Piece.PieceType.King && p.Side == Piece.PieceSide.White)
                        WhiteKing = p;
                    if (p.Type == Piece.PieceType.King && p.Side == Piece.PieceSide.Black)
                        BlackKing = p;
                }
            }
        }
        void PlacePiece(Piece piece, Square square)
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                int dX = midX - piece.Image.Width / 2;
                int dY = midY - piece.Image.Height / 2;
                piece.Location = new Point(dX, dY);
                g.DrawImageUnscaled(piece.Image, dX, dY);
            }
            if (piece.Square != null && piece.Square.Piece != null)
                piece.Square.Piece = null;
            piece.Square = square;
            square.Piece = piece;
        }
        void PlacePiece(Piece piece, Square square, String EDIT)
        {
            if (EDIT == "")
            {
                PlacePiece(piece, square);
                return;
            }
            piece.Square = square;
            square.EDPiece = piece;
        }
        void PlacePiece(Piece piece, Square square, Form1 form1)
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                int dX = midX - piece.Image.Width / 2;
                int dY = midY - piece.Image.Height / 2;
                piece.Location = new Point(dX, dY);
                g.DrawImageUnscaled(piece.Image, dX, dY);
            }
        }
        void PlacePiece(Piece piece, Square square, bool isPlayOut)
        {
            if (!isPlayOut)
            {
                using (Graphics g = panel1.CreateGraphics())
                {
                    int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                    int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                    int dX = midX - piece.Image.Width / 2;
                    int dY = midY - piece.Image.Height / 2;
                    piece.Location = new Point(dX, dY);
                    g.DrawImageUnscaled(piece.Image, dX, dY);
                }
            }
            if (piece.Square != null && piece.Square.Piece != null)
                piece.Square.Piece = null;
            piece.Square = square;
            square.Piece = piece;
        }
        void PlacePiece(Piece piece, Point location)
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                g.DrawImageUnscaled(piece.Image, location);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            //InitializeSquares();
            if (state.Game == null || state.Game.CurrentPosition == null)
                StartNewGame(false);
            else
            {
                ReInsertLabels();
                CurrentPosition = state.Game.CurrentPosition;
                LoadInitPosition();
            }
        }
        static Square GetSquare(String value)
        {            
            foreach (Square item in Squares)
            {
                if (item.Name == value)
                    return item;
            }
            return null;
        }
        static bool TryGetSquare(String value, out Square square)
        {
            foreach (Square item in Squares)
            {
                if (item.Name == value)
                {
                    square = item;
                    return true;
                }
            }
            square = null;
            return false;
        }
        static bool TryGetSquare(Engine engine, String value, out Square square)
        {
            if (engine == null)
                return TryGetSquare(value, out square);
            foreach (Square item in engine.PVSquares)
            {
                if (item.Name == value)
                {
                    square = item;
                    return true;
                }
            }
            square = null;
            return false;
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (Squares != null)
                using (Graphics g = panel1.CreateGraphics())
                {
                    g.InterpolationMode = InterpolationMode.Low;
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                    g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                    List<Square> tempList = new List<Square>();
                    foreach (Square item in Squares)
                    {
                        if (item == null || item.Rectangle == null
                            || !e.ClipRectangle.IntersectsWith(item.Rectangle))
                            continue;
                        tempList.Add(item);
                    }
                    RefreshSquares(tempList, RedrawPerspective.None, PieceClicked);
                }
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (ShouldDrawUserArrows && !isShift && !isCtrl && !isAlt)
                {
                    List<Square> list = new List<Square>();
                    foreach (var item in CurrentPosition.Lines)
                    {
                        list.AddRange(item.Squares);
                        item.Enabled = false;
                    }
                    if (settings.appearance.SaveUserArrows != "Save with position")
                    {
                        CurrentPosition.Lines.Clear();
                    }
                    if (list.Count > 0)
                        RefreshSquares(list, RedrawPerspective.UserArrows, null);
                }
                foreach (Square item in Squares)
                {
                    if (item.Rectangle.Contains(e.Location))
                    {
                        if (!isInfiniteSearch && !BestMoveArrow.Enabled && (isShift || isAlt || isCtrl))
                        {
                            DrawnLine = new Arrow(isCtrl ? CtrlColor : (isShift ? ShiftColor : 
                                (isAlt ? AltColor : new Color())));
                            DrawnLine.StartingSquare = item;
                            IsClicked = false;
                            PieceClicked = null;
                            return;
                        }
                        if (ShouldHLLegalSquares && isHighlighted)
                        {
                            if (HighlightedSquares.Contains(item))
                            {
                                ClearHighlights(HighlightedSquares);

                                if (isInfiniteSearch)
                                    foreach (var loadedE in LoadedEngines)
                                    {
                                        if (loadedE.isAnalyzing)
                                        {
                                            loadedE.Process.StandardInput.WriteLine("stop");
                                        }
                                    }
                                MakeMove(new Move(SelectedPiece, item),
                                    panel1.CreateGraphics(), false, this);

                                if (isInfiniteSearch)
                                {
                                    foreach (var LoadE in LoadedEngines)
                                    {
                                        SendPositionToEngine(LoadE, "");
                                        if (LoadE.isAnalyzing)
                                        {
                                            LoadE.Process.StandardInput.WriteLine("go infinite");
                                            LoadE.ShouldIgnore = false;
                                        }
                                    }
                                }
                                return;
                            }
                            else if (item.Piece == null || item.Piece.Side != sideToPlay)
                            {
                                ClearHighlights(HighlightedSquares);
                            }
                        }
                        if (item.Piece != null)
                        {
                            PieceClicked = item.Piece;
                            SelectedPiece = item.Piece;
                            PointClicked = e.Location;
                            UpdateArrow(true);
                            IsClicked = true;
                        }
                        break;
                    }
                }
            }
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsClicked)
            {
                if (PieceClicked == null)
                {
                    IsClicked = false;
                    return;
                }
                if (isHighlighted)
                {
                    ClearHighlights(HighlightedSquares);
                }
                Rectangle tempRect = new Rectangle(PieceClicked.Location, PieceClicked.Image.Size);
                List<Square> tempList = new List<Square>();
                foreach (Square item in Squares)
                {
                    using (Graphics g = panel1.CreateGraphics())
                    {
                        if (item.Rectangle.IntersectsWith(tempRect))
                        {
                            tempList.Add(item);
                        }
                    }
                }
                RefreshSquares(tempList, RedrawPerspective.None, PieceClicked);
                Point diff = Point.Subtract(e.Location, (Size)PointClicked);
                PieceClicked.Location = PieceClicked.Location + (Size)diff;
                PlacePiece(PieceClicked, PieceClicked.Location);
                PointClicked = e.Location;
            }
            else
            {

            }
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            if (!isInfiniteSearch && !BestMoveArrow.Enabled && DrawnLine != null &&
                            DrawnLine.StartingSquare != null)
                foreach (var item in Squares)
                    if (item.Rectangle.Contains(e.Location))
                    {
                        DrawnLine.StoppingSquare = item;
                        int x = CurrentPosition.Lines.IndexOf(DrawnLine);
                        if (x >= 0)
                        {
                            Arrow arrow = CurrentPosition.Lines[x];
                            CurrentPosition.Lines.RemoveAt(x);
                            RefreshSquares(arrow.Squares, RedrawPerspective.Arrow, null);
                            DrawnLine = null;
                            return;
                        }
                        CurrentPosition.Lines.Add(DrawnLine);
                        DrawnLine.Enabled = true;
                        UpdateArrow(DrawnLine, false);
                        DrawnLine = null;
                        return;
                    }
                        
            IsClicked = false;
            if (isInfiniteSearch)
            {
                ArrowTimer.Enabled = true;
                ArrowTimer.Start(); 
            }
            if (PieceClicked == null)
                return;
            Rectangle tempRect = new Rectangle(PieceClicked.Location, PieceClicked.Image.Size);
            Point midPoint = new Point();
            midPoint.X = PieceClicked.Location.X + PieceClicked.Image.Width / 2;
            midPoint.Y = PieceClicked.Location.Y + PieceClicked.Image.Height / 2;
            if (!panel1.DisplayRectangle.Contains(e.Location))
            {
                List<Square> list = new List<Square>();
                list.Add(PieceClicked.Square);
                RefreshSquares(list, RedrawPerspective.None, null);
                PieceClicked = null;
            }
            foreach (Square square in Squares)
            {
                using (Graphics g = panel1.CreateGraphics())
                {
                    if (square.Rectangle.Contains(midPoint))
                    {
                        if (!CheckMove(new Move(PieceClicked, square)))
                        {
                            if (square.Rectangle.IntersectsWith(tempRect))
                            {
                                List<Square> list = new List<Square>();
                                list.Add(square);
                                RefreshSquares(list, RedrawPerspective.None, PieceClicked);
                            }
                            PlacePiece(PieceClicked, PieceClicked.Square);
                            if (ShouldHLLegalSquares)
                                HighlightSquares(SelectedPiece);
                        }
                        else
                        {
                            if (isInfiniteSearch)
                                foreach (var item in LoadedEngines)
                                {
                                    if (item.isAnalyzing)
                                    {
                                        item.Process.StandardInput.WriteLine("stop");
                                    }
                                }

                            MakeMove(new Move(PieceClicked.Square, PieceClicked, square), g, false);

                            if (isInfiniteSearch)
                            {
                                foreach (var item in LoadedEngines)
                                {
                                    SendPositionToEngine(item, "");
                                    if (item.isAnalyzing)
                                    {
                                        item.Process.StandardInput.WriteLine("go infinite");
                                        item.ShouldIgnore = false;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (square.Rectangle.IntersectsWith(tempRect))
                        {
                            List<Square> list = new List<Square>();
                            list.Add(square);
                            RefreshSquares(list, RedrawPerspective.None, PieceClicked);
                        }
                    }
                }
            }
            PieceClicked = null;
        }
        private void HighlightSquare(Square item)
        {
            if (!ShouldHLLegalSquares)
                return;
            if (item.Piece != null)
            {
                using (Graphics g = panel1.CreateGraphics())
                {
                    SolidBrush brush = new SolidBrush(LegalMoveHLColor);
                    GraphicsPath p1 = new GraphicsPath();
                    GraphicsPath p2 = new GraphicsPath();
                    Rectangle rect = item.Rectangle;
                    p1.AddRectangle(rect);
                    p2.AddEllipse(new Rectangle(new Point((rect.X - 1), rect.Y), rect.Size));
                    Region region = new Region(p1);
                    region.Exclude(p2);
                    g.FillRegion(brush, region);
                }
            }
            else
            {
                FillCircle(new SolidBrush(LegalMoveHLColor),
                    item.Rectangle, 8);
            }
        }
        private void HighlightSquares(Piece piece)
        {
            if (piece.Side != sideToPlay || IsWhiteCheckmated || IsBlackCheckmated)
            {
                return;
            }
            if (isHighlighted)
            {
                ClearHighlights(HighlightedSquares);
                if (LastHighlightedPiece != null && LastHighlightedPiece == piece)
                    return;
            }
            ValidSquares = GetValidSquares(piece);

            if (piece.Type == Piece.PieceType.King)
                KingCrossCheck(piece);
            else
                PieceCrossCheck(piece);

            if (IsWhiteInCheck || IsBlackInCheck)
            {
                HighlightedSquares.Clear();
                foreach (var item in ValidSquares)
                {
                    if (PossibleDefences.Contains(new Move(piece, item)))
                        HighlightedSquares.Add(item);
                }
            }
            else
                HighlightedSquares = ValidSquares;

            if (HighlightedSquares.Count > 0)
            {
                isHighlighted = true;
                LastHighlightedPiece = piece;

                bool buffer = ShouldHLLegalSquares;
                ShouldHLLegalSquares = false;
                if (ShouldHighlightLastMove && LastMoveHighlighted != null)
                {
                    List<Square> list = new List<Square>();
                    list.Add(LastMoveHighlighted.OriginalSquare);
                    list.Add(LastMoveHighlighted.DestSquare);
                    RefreshSquares(list, RedrawPerspective.LastMoveHL, null);
                }
                ShouldHLLegalSquares = buffer;
            }
            else
                isHighlighted = false;
            foreach (var item in HighlightedSquares)
            {
                HighlightSquare(item);
            }
        }
        private void ClearHighlights(List<Square> tempList)
        {
            isHighlighted = false;
            RefreshSquares(tempList, RedrawPerspective.None, null);
            tempList.Clear();
            if (LastMoveHighlighted == null)
                return;
            if (ShouldHighlightLastMove && CurrentPosition.LastMovePlayed != null)
            {
                HighLightLastMove(CurrentPosition.LastMovePlayed);
            }
        }
        private void CheckForCheck(Piece piece)
        {
            if (sideToPlay != piece.Side)
            {
                List<Square> temp = GetValidSquares(piece);
                foreach (Square item in temp)
                {
                    if (item.Piece != null && item.Piece.Type == Piece.PieceType.King
                        && item.Piece.Side != piece.Side)
                    {
                        lock (locker)
                        {
                            if (CheckingPiece == null)
                                CheckingPiece = piece;
                            else if (DoubleCheckingPiece == null)
                                DoubleCheckingPiece = piece;
                        }

                        if (piece.Side == Piece.PieceSide.White)
                            IsBlackInCheck = true;
                        else
                            IsWhiteInCheck = true;
                    }
                }
            }
        }
        private void MakeMove(Move move, Graphics g, bool isPlayOut, Form1 form1)
        {
            if (isInfiniteSearch)
            {
                UpdateArrow(true);
                BestMoveArrow.IsInvalid = true;
                ArrowTimer.Enabled = true;
                ArrowTimer.Start();
            }

            Position nextPosition = new Position();
            MoveCount++;
            bool isPieceCaptured = false;
            if (!isPlayOut)
                if ((CurrentVariation.MovesList.IndexOf(CurrentPosition) < CurrentVariation.MovesList.Count - 1) &&
                    !(move.PieceMoving.Type == Piece.PieceType.Pawn &&
                    (int.Parse(move.DestSquare.Name.Substring(1, 1)) == 1 ||
                    int.Parse(move.DestSquare.Name.Substring(1, 1)) == 8)))
                {
                    nextPosition = CurrentVariation.MovesList[CurrentVariation.MovesList.IndexOf(CurrentPosition) + 1];
                    if (!move.Equals(nextPosition.LastMovePlayed))
                    {
                        if (nextPosition.VariationHolders != null)
                            foreach (var item in nextPosition.VariationHolders)
                            {
                                if (item.MovesList[0].LastMovePlayed.Equals(move))
                                {
                                    CurrentVariation = item;
                                    g.FillRectangle(move.DestSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                                        LightSquareColor, move.DestSquare.Rectangle);
                                    LoadPosition(item.MovesList[0], isPlayOut);
                                    UpdateNotationLabels();
                                    return;
                                }
                            }

                        if (settings.behaviour.NewMoveHandling == "Always ask")   //Move is really new at this point
                        {
                            ShowNewVariationDialog();
                            if (newVarOption == NewVariationOption.Cancel)
                            {
                                MoveCount--;
                                g.FillRectangle(move.DestSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                                    LightSquareColor, move.DestSquare.Rectangle);
                                PlacePiece(move.PieceMoving, move.OriginalSquare, false);
                                newVarOption = NewVariationOption.NotDefined;
                                return;
                            }
                        }

                        else
                        {
                            if (settings.behaviour.NewMoveHandling == "Always new variation")
                                newVarOption = NewVariationOption.NewVariation;
                            if (settings.behaviour.NewMoveHandling == "Always overwrite")
                                newVarOption = NewVariationOption.Overwrite;
                            if (settings.behaviour.NewMoveHandling == "Always new main line")
                                newVarOption = NewVariationOption.NewMainLine;
                        }

                    }

                    else
                    {
                        ReplayOldMove(move, g, nextPosition);
                        return;
                    }
                }

            if (IsWhiteInCheck || IsBlackInCheck)
            {
                IsWhiteInCheck = false;
                IsBlackInCheck = false;
                CheckingPiece = null;
                DoubleCheckingPiece = null;
            }
            Square OriginalSq = move.PieceMoving.Square;
            bool IsCastled = HandleCastling(move.PieceMoving, move.DestSquare, g, isPlayOut, form1);

            if (move.PieceMoving.Type == Piece.PieceType.Pawn && (int.Parse(move.DestSquare.Name.Substring(1, 1)) == 8 ||
                int.Parse(move.DestSquare.Name.Substring(1, 1)) == 1))
            {
                HandlePawnPromotion(move, g, isPlayOut);
                return;
            }

            if (move.PieceMoving.Type == Piece.PieceType.Pawn)
            {
                if (!HandleEnPassant(move.PieceMoving, move.DestSquare, out isPieceCaptured))
                    EnPassantPawn = null;
                move.IsCapture = isPieceCaptured;
            }

            else
                EnPassantPawn = null;
            if (move.DestSquare.Piece != null && !IsCastled)
                move.IsCapture = true;
            move.GetShortNotation();
            if (!IsCastled)
            {
                if (move.DestSquare.Piece != null)
                {
                    CapturedPieces.Add(move.DestSquare.Piece);
                    move.DestSquare.Piece.Square = null;
                    Pieces.Remove(move.DestSquare.Piece);
                    isPieceCaptured = true;
                }
                if (!isPlayOut && form1 == null)
                {
                    g.FillRectangle(move.DestSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, move.DestSquare.Rectangle);
                }
                if (!ShouldAnimate || form1 == null || isPlayOut)
                {
                    if (!isPlayOut)
                    {
                        List<Square> list = new List<Square>();
                        list.Add(move.OriginalSquare);
                        RefreshSquares(list, RedrawPerspective.None, OriginalSq.Piece);
                    }
                    PlacePiece(move.PieceMoving, move.DestSquare, isPlayOut);
                }
                else if (!ShouldAnimate)
                {
                    
                }
                else
                {
                    IsAnimating = true;
                    AnimationList = new List<AnimationTask>();
                    AnimationList.Add(new AnimationTask(move));
                    if (move.PieceMoving.Square != null && move.PieceMoving.Square.Piece != null)
                        move.PieceMoving.Square.Piece = null;
                    move.PieceMoving.Square = move.DestSquare;
                    move.DestSquare.Piece = move.PieceMoving;
                    AnimationTimer.Start();
                }
            }
            sideToPlay = (sideToPlay == Piece.PieceSide.White) ? Piece.PieceSide.Black
                : Piece.PieceSide.White;
            HandleChecking(isPlayOut);
            if (!isPlayOut)
                HandleDraws(move);
            
            move.ShortNotation += ((!(IsWhiteCheckmated || IsBlackCheckmated) && 
                (IsWhiteInCheck || IsBlackInCheck)) ? "+" : "") + ((IsWhiteCheckmated || IsBlackCheckmated) ? "#" : "");

            VariationHolder tempVariation = new VariationHolder();
            if (newVarOption == NewVariationOption.NewVariation)
            {
                if (nextPosition.VariationHolders == null)
                    nextPosition.VariationHolders = new List<VariationHolder>();
                VariationHolder tempVarHolder = new VariationHolder(new List<Position>(), CurrentVariation);
                tempVarHolder.ParentIndex = CurrentVariation.MovesList.IndexOf(CurrentPosition);
                tempVarHolder.ListOfParents = new List<VariationHolder>();

                VariationHolder CurrentNode = tempVarHolder.ParentLine;
                bool ShouldContinue = true;
                while (ShouldContinue)
                {
                    tempVarHolder.ListOfParents.Add(CurrentNode);
                    if (CurrentNode.ParentLine != null)
                        CurrentNode = CurrentNode.ParentLine;
                    else ShouldContinue = false;
                }

                CurrentVariation = tempVarHolder;
                nextPosition.VariationHolders.Add(tempVarHolder);
                GameVariations.Add(tempVarHolder);
            }

            else if (newVarOption == NewVariationOption.NewMainLine)
            {
                InsertNewMove(nextPosition, true, out tempVariation);
            }
            else if (newVarOption == NewVariationOption.Overwrite)
            {
                InsertNewMove(nextPosition, false, out tempVariation);
            }

            SavePosition(move, isPlayOut);
            if (tempVariation != null && newVarOption == NewVariationOption.NewMainLine)
            {
                CurrentPosition.VariationHolders = new List<VariationHolder>();
                CurrentPosition.VariationHolders.Add(tempVariation);
            }
            if (newVarOption == NewVariationOption.NewMainLine || newVarOption == NewVariationOption.Overwrite)
                ReInsertLabels();
            newVarOption = NewVariationOption.NotDefined;
        }
        private void MakeMove(Move move, Graphics g, bool isPlayOut)
        {
            MakeMove(move, g, isPlayOut, null);
        }
        private void InsertNewMove(Position nextPosition, bool KeepOldMoveAsVariation, out VariationHolder varholder)
        {
            int x = CurrentVariation.MovesList.IndexOf(nextPosition),
                y = CurrentVariation.MovesList.Count - x;
            List<Position> positionList = new List<Position>();

            if (!KeepOldMoveAsVariation)        // For Overwrite
            {
                List<VariationHolder> tempList = new List<VariationHolder>();
                foreach (var item in GameVariations)
                {
                    if (item.ParentLine == CurrentVariation &&
                        item.ParentIndex >= CurrentVariation.MovesList.IndexOf(CurrentPosition))
                        tempList.Add(item);
                }
                foreach (var item in tempList)
                    GameVariations.Remove(item);

                CurrentVariation.MovesList.RemoveRange(x, y);
                varholder = null;
            }

            else            // For New Main Line
            {
                List<Position> tempMovesList = new List<Position>();
                VariationHolder tempVarHolder;
                for (int i = 0; i < y; i++)
                {
                    tempMovesList.Add(CurrentVariation.MovesList[x + i]);
                }
                tempVarHolder = new VariationHolder(tempMovesList, CurrentVariation);
                tempVarHolder.ParentIndex = CurrentVariation.MovesList.IndexOf(CurrentPosition);

                VariationHolder CurrentNode = tempVarHolder.ParentLine;
                bool ShouldContinue = true;
                while (ShouldContinue)
                {
                    tempVarHolder.ListOfParents.Add(CurrentNode);
                    if (CurrentNode.ParentLine != null)
                        CurrentNode = CurrentNode.ParentLine;
                    else ShouldContinue = false;
                }

                foreach (var item in tempMovesList)
                {
                    if (item.VariationHolders != null)
                    {
                        foreach (var item2 in item.VariationHolders)
                        {
                            item2.ParentLine = tempVarHolder;
                            item2.ParentIndex = tempMovesList.IndexOf(item) - 1;
                        }
                    }
                }
                CurrentVariation.MovesList.RemoveRange(x, y);
                varholder = tempVarHolder;
            }
        }
        private void InsertNewMainMove(Position nextPosition, bool KeepOldMoveAsVariation, out VariationHolder varholder)
        {
            int x = CurrentVariation.MovesList.IndexOf(nextPosition),
                y = CurrentVariation.MovesList.Count - x;
            List<Label> LabelList = new List<Label>();
            List<Position> positionList = new List<Position>();

            foreach (var item in flowLayoutPanel1.Controls)
            {
                Label label = item as Label;
                VariationHolder vh = (VariationHolder)label.Tag;
                if (focusLabel == null)
                {
                    LabelList.Add(label);
                }
                else if (vh == CurrentVariation
                    && flowLayoutPanel1.Controls.GetChildIndex(label) > flowLayoutPanel1.Controls.GetChildIndex(focusLabel))
                {
                    if (vh.ParentLine == null || label.Name != "Y")
                        LabelList.Add(label);
                }
                else if (vh.ParentLine == CurrentVariation
                    && flowLayoutPanel1.Controls.GetChildIndex(label) > flowLayoutPanel1.Controls.GetChildIndex(focusLabel))
                {
                    LabelList.Add(label);
                }
            }
            if (!KeepOldMoveAsVariation)        // For Overwrite
            {
                List<VariationHolder> tempList = new List<VariationHolder>();
                foreach (var item in GameVariations)
                {
                    if (item.ParentLine == CurrentVariation &&
                        item.ParentIndex >= CurrentVariation.MovesList.IndexOf(CurrentPosition))
                        tempList.Add(item);
                }
                foreach (var item in tempList)
                    GameVariations.Remove(item);

                CurrentVariation.MovesList.RemoveRange(x, y);
                flowLayoutPanel1.SuspendLayout();
                foreach (var item in LabelList)
                {
                    flowLayoutPanel1.Controls.Remove(item);
                }
                flowLayoutPanel1.ResumeLayout();
                varholder = null;
            }

            else            // For New Main Line
            {
                List<Position> tempMovesList = new List<Position>();
                VariationHolder tempVarHolder;
                for (int i = 0; i < y; i++)
                {
                    tempMovesList.Add(CurrentVariation.MovesList[x + i]);
                }
                tempVarHolder = new VariationHolder(tempMovesList, CurrentVariation);
                tempVarHolder.ParentIndex = CurrentVariation.MovesList.IndexOf(CurrentPosition);

                VariationHolder CurrentNode = tempVarHolder.ParentLine;
                bool ShouldContinue = true;
                while (ShouldContinue)
                {
                    tempVarHolder.ListOfParents.Add(CurrentNode);
                    if (CurrentNode.ParentLine != null)
                        CurrentNode = CurrentNode.ParentLine;
                    else ShouldContinue = false;
                }

                foreach (var item in tempMovesList)
                {
                    if (item.VariationHolders != null)
                    {
                        foreach (var item2 in item.VariationHolders)
                        {
                            item2.ParentLine = tempVarHolder;
                            item2.ParentIndex = tempMovesList.IndexOf(item) - 1;
                        }
                    }
                }
                CurrentVariation.MovesList.RemoveRange(x, y);
                varholder = tempVarHolder;

                foreach (var item in LabelList)
                {
                    item.ForeColor = NotationSubForeColor;
                    if (item.Text == " [ ")
                        item.Text = "(";
                    if (item.Text == " ] ")
                        item.Text = ")";
                    if (item.Tag == CurrentVariation)
                        item.Tag = tempVarHolder;
                    flowLayoutPanel1.SetFlowBreak(item, false);
                }

                Label braceLabel1 = new Label();
                Label braceLabel2 = new Label();
                braceLabel1.Tag = tempVarHolder;
                braceLabel2.Tag = tempVarHolder;
                braceLabel1.Name = "X";
                braceLabel2.Name = "Y";
                if (CurrentVariation == MainLine)
                {
                    braceLabel1.Text = " [ ";
                    braceLabel2.Text = " ] ";
                }
                else
                {
                    braceLabel1.Text = "(";
                    braceLabel2.Text = ")";
                }
                braceLabel1.Font = NotationFont;
                braceLabel2.Font = NotationFont;
                braceLabel1.AutoSize = true;
                braceLabel2.AutoSize = true;
                flowLayoutPanel1.Controls.Add(braceLabel1);
                flowLayoutPanel1.Controls.Add(braceLabel2);
                flowLayoutPanel1.Controls.SetChildIndex(braceLabel1, flowLayoutPanel1.Controls.IndexOf(focusLabel) + 1);
                flowLayoutPanel1.Controls.SetChildIndex(braceLabel2, flowLayoutPanel1.Controls.IndexOf(braceLabel1) + 1);



                if (LabelList[0].Name[0] == 'B')
                {
                    String str = LabelList[0].Text;
                    LabelList[0].Text = LabelList[0].Name.Substring(1, LabelList[0].Name.Length - 1) + " . . . ";
                    LabelList[0].Text += str;
                }

                Label label = braceLabel1;
                foreach (var item in LabelList)
                {
                    flowLayoutPanel1.Controls.SetChildIndex(item, flowLayoutPanel1.Controls.IndexOf(label) + 1);
                    label = item;
                }
                if (CurrentVariation == MainLine)
                {
                    flowLayoutPanel1.SetFlowBreak(braceLabel2, true);
                }

                flowLayoutPanel1.Refresh();
            }

        }
        private void ReplayOldMove(Move move, Graphics g, Position nextPosition)
        {
            g.FillRectangle(move.DestSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                LightSquareColor, move.DestSquare.Rectangle);
            LoadPosition(nextPosition, false);

            if (focusLabel != null)
            {
                focusLabel.Font = NotationFont;
                if (focusLabel.Tag == MainLine)
                    focusLabel.ForeColor = NotationMainForeColor;
                else
                    focusLabel.ForeColor = NotationSubForeColor;
            }

            foreach (var itemx in flowLayoutPanel1.Controls)
            {
                Label item = itemx as Label;
                if (item.Name.Substring(1, item.Name.Length - 1) == CurrentPosition.LastMovePlayed.MoveNo.ToString() &&
                    (VariationHolder)item.Tag == CurrentVariation)
                {
                    if ((CurrentPosition.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White && item.Name[0] == 'W')
                        || CurrentPosition.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.Black && item.Name[0] == 'B')
                    {
                        focusLabel = item;
                        focusLabel.Font = NotationFont;
                        focusLabel.ForeColor = NotationFocusColor;
                        flowLayoutPanel1.Refresh();
                        break;
                    }
                }
            }
            return;
        }
        private void UpdateNotationLabels()
        {
            if (focusLabel != null)
            {
                focusLabel.Font = NotationFont;
                if (focusLabel.Tag == MainLine)
                    focusLabel.ForeColor = NotationMainForeColor;
                else
                    focusLabel.ForeColor = NotationSubForeColor;
            }

            foreach (var itemX in flowLayoutPanel1.Controls)
            {
                Label itemY = itemX as Label;
                if (itemY.Name == "X" || itemY.Name == "Y")
                    continue;
                if ((VariationHolder)itemY.Tag == CurrentVariation)
                {
                    focusLabel = itemY;
                    focusLabel.Font = NotationFont;
                    focusLabel.ForeColor = NotationFocusColor;
                    flowLayoutPanel1.Refresh();
                    break;
                }
            }
        }
        private void ShowNewVariationDialog()
        {
            NewVariationForm = new Form();
            Button NewVariationButton = new Button(), OverwriteButton = new Button(),
                NewMainLineButton = new Button(), CancelButton = new Button();
            CheckBox SetAsDefaultCheckBox = new CheckBox();
            SetAsDefaultCheckBox.Text = "Set as Default";
            NewVariationButton.Text = "New Variation";
            OverwriteButton.Text = "Overwrite";
            NewMainLineButton.Text = "New Main Line";
            CancelButton.Text = "Cancel";
            OverwriteButton.Size = new System.Drawing.Size(150, 30);
            OverwriteButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            NewVariationButton.Size = new System.Drawing.Size(150, 30);
            NewVariationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            NewMainLineButton.Size = new System.Drawing.Size(150, 30);
            NewMainLineButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Regular);
            CancelButton.Size = new System.Drawing.Size(100, 30);
            CancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Regular);

            NewVariationButton.Click += NewVariationForm_Click;
            OverwriteButton.Click += NewVariationForm_Click;
            NewMainLineButton.Click += NewVariationForm_Click;
            CancelButton.Click += NewVariationForm_Click;

            NewVariationButton.Location = new Point(50, 50);
            OverwriteButton.Location = new Point(50, 90);
            NewMainLineButton.Location = new Point(50, 130);
            SetAsDefaultCheckBox.Location = new Point(50, 200);
            CancelButton.Location = new Point(50, 240);
            NewVariationForm.Width = CancelButton.Location.X + 220;
            NewVariationForm.Height = CancelButton.Location.Y + 100;
            NewVariationForm.MaximizeBox = false;
            NewVariationForm.MinimizeBox = false;
            NewVariationForm.ShowIcon = false;       // Maybe change later
            NewVariationForm.Text = "New Move";

            NewVariationForm.Controls.Add(NewVariationButton);
            NewVariationForm.Controls.Add(OverwriteButton);
            NewVariationForm.Controls.Add(NewMainLineButton);
            NewVariationForm.Controls.Add(CancelButton);
            NewVariationForm.Controls.Add(SetAsDefaultCheckBox);
            NewVariationForm.StartPosition = FormStartPosition.CenterParent;

            ArrowTimer.Enabled = false;
            ArrowTimer.Stop();
            UpdateArrow(true);

            NewVariationForm.ShowDialog(this);
            ArrowTimer.Enabled = true;
            ArrowTimer.Start();

            if (newVarOption == NewVariationOption.NotDefined)
                newVarOption = NewVariationOption.Cancel;
            if (SetAsDefaultCheckBox.Checked)
            {
                switch (newVarOption)
                {
                    case NewVariationOption.NewVariation:
                        settings.behaviour.NewMoveHandling = "Always new variation";
                        break;
                    case NewVariationOption.NewMainLine:
                        settings.behaviour.NewMoveHandling = "Always new main line";
                        break;
                    case NewVariationOption.Overwrite:
                        settings.behaviour.NewMoveHandling = "Always overwrite";
                        break;
                }
            }
        }
        private void NewVariationForm_Click(object sender, EventArgs e)
        {
            Button tempButton = sender as Button;
            if (tempButton.Text == "Overwrite")
            {
                newVarOption = NewVariationOption.Overwrite;
                NewVariationForm.Close();
            }
            else if (tempButton.Text == "New Variation")
            {
                newVarOption = NewVariationOption.NewVariation;
                NewVariationForm.Close();
            }
            else if (tempButton.Text == "New Main Line")
            {
                newVarOption = NewVariationOption.NewMainLine;
                NewVariationForm.Close();
            }
            else if (tempButton.Text == "Cancel")
            {
                newVarOption = NewVariationOption.Cancel;
                NewVariationForm.Close();
            }
        }
        private void HandleDraws(Move move)
        {
            if (IsWhiteCheckmated || IsBlackCheckmated || IsDraw)
                return;
            if (move != null && move.PieceMoving.Type != Piece.PieceType.Pawn
                && !move.IsCapture)      // 50 Move rule
                FiftyMoveCount++;
            else
                FiftyMoveCount = 0;
            if (FiftyMoveCount == 100)
            {
                IsDraw = true;
                if (CurrentVariation == MainLine)
                {
                    gameDetails.Result = GameDetails.Outcome.Draw;
                    OnGameEnding();
                    ShowGameDetails();
                }
                ShouldClockTick = false;
                ModeOfPlay = PlayMode.EditPosition;
                if (isEngineMatchInProgress)
                {
                    isEngineMatchInProgress = false;
                    foreach (var item in LoadedEngines)
                    {
                        if (!item.isUciEngine)
                            continue;
                        item.Process.StandardInput.WriteLine("stop");
                        item.isAnalyzing = false;
                    }
                }
                MessageBox.Show("50 Move Draw!");
                return;
            }

            bool IsStalemate = true;                        // Stalemate
            foreach (var item in Pieces)
            {
                if (item.Side == sideToPlay)
                {
                    ValidSquares = GetValidSquares(item);

                    if (item.Type == Piece.PieceType.King)
                        KingCrossCheck(item);
                    else
                        PieceCrossCheck(item);

                    if (ValidSquares.Count > 0)
                    {
                        IsStalemate = false;
                        break;
                    }
                }
            }
            if (IsStalemate == true)
            {
                IsDraw = true;
                if (CurrentVariation == MainLine)
                {
                    gameDetails.Result = GameDetails.Outcome.Draw;
                    OnGameEnding();
                    ShowGameDetails();
                }
                ShouldClockTick = false;
                ModeOfPlay = PlayMode.EditPosition;
                if (isEngineMatchInProgress)
                {
                    isEngineMatchInProgress = false;
                    foreach (var item in LoadedEngines)
                    {
                        if (!item.isUciEngine)
                            continue;
                        item.Process.StandardInput.WriteLine("stop");
                        item.isAnalyzing = false;
                    }
                }
                MessageBox.Show("Stalemate!");
                return;
            }

            // Insufficient Material rule
            bool Sufficient = false, firstWhiteMinorPiece = false, firstBlackMinorPiece = false;
            foreach (var item in Pieces)
            {
                if (item.Type == Piece.PieceType.Pawn || item.Type == Piece.PieceType.Rook 
                    || item.Type == Piece.PieceType.Queen)
                {
                    Sufficient = true;
                    break;
                }
                if (item.Type == Piece.PieceType.Bishop || item.Type == Piece.PieceType.Knight)
                {
                    if (firstWhiteMinorPiece && item.Side == Piece.PieceSide.White)
                    {
                        Sufficient = true;
                        break;
                    }
                    if (firstBlackMinorPiece && item.Side == Piece.PieceSide.Black)
                    {
                        Sufficient = true;
                        break;
                    }
                    if (!firstWhiteMinorPiece && item.Side == Piece.PieceSide.White)
                        firstWhiteMinorPiece = true;
                    else if (!firstBlackMinorPiece && item.Side == Piece.PieceSide.Black)
                        firstBlackMinorPiece = true;
                }
            }
            if (!Sufficient)
            {
                IsDraw = true;
                if (CurrentVariation == MainLine)
                {
                    gameDetails.Result = GameDetails.Outcome.Draw;
                    OnGameEnding();
                    ShowGameDetails();
                }
                ShouldClockTick = false;
                ModeOfPlay = PlayMode.EditPosition;
                if (isEngineMatchInProgress)
                {
                    isEngineMatchInProgress = false;
                    foreach (var item in LoadedEngines)
                    {
                        if (!item.isUciEngine)
                            continue;
                        item.Process.StandardInput.WriteLine("stop");
                        item.isAnalyzing = false;
                    }
                }
                MessageBox.Show("Draw due to insufficient material");
                return;
            }
        }
        private bool HandleEnPassant(Piece piece, Square square, out bool IsPieceCapture)
        {
            if (int.Parse(piece.Square.Name.Substring(1, 1)) == 2 &&
                (int.Parse(square.Name.Substring(1, 1)) == 4))                  //White advance
            {
                EnPassantPawn = piece;
                IsPieceCapture = false;
                return true;
            }
            else if (int.Parse(piece.Square.Name.Substring(1, 1)) == 7 &&
                (int.Parse(square.Name.Substring(1, 1)) == 5))                  //Black advance
            {
                EnPassantPawn = piece;
                IsPieceCapture = false;
                return true;
            }
            else
            {
                if (piece.Square.Name[0] - square.Name[0] != 0 && square.Piece == null)
                {
                    CapturedPieces.Add(EnPassantPawn);
                    using (Graphics g = panel1.CreateGraphics())
                    {
                        g.FillRectangle(EnPassantPawn.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, EnPassantPawn.Square.Rectangle);
                    }
                    EnPassantPawn.Square.Piece = null;
                    EnPassantPawn.Square = null;
                    Pieces.Remove(EnPassantPawn);
                    IsPieceCapture = true;
                }
                else
                    IsPieceCapture = false;
            }
            return false;
        }
        private void HandleChecking(bool isPlayOut)
        {
            Parallel.ForEach(Pieces, CheckForCheck);
            if (IsWhiteInCheck || IsBlackInCheck)
            {
                PossibleDefences = new List<Move>();
                if (IsWhiteInCheck)
                {
                    ValidSquares = GetValidSquares(WhiteKing);
                    KingCrossCheck(WhiteKing);
                    foreach (Square sq in ValidSquares)
                        PossibleDefences.Add(new Move(WhiteKing, sq));
                    if (DoubleCheckingPiece != null)
                    {
                        if (PossibleDefences.Count == 0)
                        {
                            IsWhiteCheckmated = true;
                            if (CurrentVariation == MainLine)
                            {
                                gameDetails.Result = GameDetails.Outcome.BlackWin;
                                ShowGameDetails();
                            }
                            ShouldClockTick = false;
                        }
                        return;
                    }

                    AttemptCapturingChecker();

                    if (CheckingPiece.Type != Piece.PieceType.Knight &&
                        CheckingPiece.Type != Piece.PieceType.Pawn)
                    {
                        PossibleBlockingSquares = GetBlockingSquares((IsWhiteInCheck ? WhiteKing : BlackKing),
                            CheckingPiece);
                        AttemptBlocking();
                    }
                }

                else
                {
                    ValidSquares = GetValidSquares(BlackKing);
                    KingCrossCheck(BlackKing);
                    foreach (Square sq in ValidSquares)
                        PossibleDefences.Add(new Move(BlackKing, sq));
                    if (DoubleCheckingPiece != null)
                    {
                        if (PossibleDefences.Count == 0)
                        {
                            IsBlackCheckmated = true;
                            if (CurrentVariation == MainLine)
                            {
                                gameDetails.Result = GameDetails.Outcome.WhiteWin;
                                ShowGameDetails();
                            }
                            ShouldClockTick = false;
                        }
                        return;
                    }
                    AttemptCapturingChecker();

                    if (CheckingPiece.Type != Piece.PieceType.Knight &&
                        CheckingPiece.Type != Piece.PieceType.Pawn)
                    {
                        PossibleBlockingSquares = GetBlockingSquares((IsWhiteInCheck ? WhiteKing : BlackKing),
                            CheckingPiece);
                        AttemptBlocking();
                    }
                }

                if (PossibleDefences.Count == 0)
                {
                    if (IsWhiteInCheck)
                    {
                        IsWhiteCheckmated = true;
                        if (CurrentVariation == MainLine)
                        {
                            gameDetails.Result = GameDetails.Outcome.BlackWin;
                            ShowGameDetails();
                        }
                    }

                    else
                    {
                        IsBlackCheckmated = true;
                        if (CurrentVariation == MainLine)
                        {
                            gameDetails.Result = GameDetails.Outcome.WhiteWin;
                            ShowGameDetails();
                        }
                    }
                }
            }
        }
        private void HandlePawnPromotion(Move move, Graphics g, bool isPlayOut)
        {
            if (settings.behaviour.PromotionType == "Always Queen" && move.PromoteType == Move.ItsPromoteType.NotAvailable)
                move.PromoteType = Move.ItsPromoteType.Queen;

            bool isWhite = int.Parse(move.DestSquare.Name.Substring(1, 1)) == 8 ? true : false;
            PromotePawn = move.PieceMoving;
            PromoteSquare = move.DestSquare;
            if (isPlayOut || move.PromoteType != Move.ItsPromoteType.NotAvailable)
            {
                switch (move.PromoteType)
                {
                    case Move.ItsPromoteType.Queen:
                        PromotePiece = new Piece();
                        PromotePiece.Type = Piece.PieceType.Queen;
                        PromotePiece.Side = isWhite ? Piece.PieceSide.White : Piece.PieceSide.Black;
                        PromotePiece.Image = isWhite ? WhiteQueenImage : BlackQueenImage;
                        break;
                    case Move.ItsPromoteType.Rook:
                        PromotePiece = new Piece();
                        PromotePiece.Type = Piece.PieceType.Rook;
                        PromotePiece.Side = isWhite ? Piece.PieceSide.White : Piece.PieceSide.Black;
                        PromotePiece.Image = isWhite ? WhiteRookImage : BlackRookImage;
                        break;
                    case Move.ItsPromoteType.Bishop:
                        PromotePiece = new Piece();
                        PromotePiece.Type = Piece.PieceType.Bishop;
                        PromotePiece.Side = isWhite ? Piece.PieceSide.White : Piece.PieceSide.Black;
                        PromotePiece.Image = isWhite ? WhiteBishopImage : BlackBishopImage;
                        break;
                    case Move.ItsPromoteType.Knight:
                        PromotePiece = new Piece();
                        PromotePiece.Type = Piece.PieceType.Knight;
                        PromotePiece.Side = isWhite ? Piece.PieceSide.White : Piece.PieceSide.Black;
                        PromotePiece.Image = isWhite ? WhiteKnightImage : BlackKnightImage;
                        break;
                    case Move.ItsPromoteType.NotAvailable:
                        MessageBox.Show("ERROR. Wrong PromoteType \n HandlePawnPromotion");
                        break;
                }
                PromoteForm_FormClosed(isPlayOut ? PromotePiece : null,
                    new FormClosedEventArgs(CloseReason.None));
                return;
            }

            PromoteForm = new Form();
            PromoteForm.Text = "Promotion";
            PromoteForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            PromoteForm.MaximizeBox = false;
            PromoteForm.MinimizeBox = false;
            PromoteForm.ShowIcon = false;
            PromoteForm.StartPosition = FormStartPosition.Manual;
            PromoteForm.BackColor = Color.Gray;
            PictureBox QueenBox = new PictureBox();
            PictureBox RookBox = new PictureBox();
            PictureBox BishopBox = new PictureBox();
            PictureBox KnightBox = new PictureBox();
            QueenBox.Image = isWhite ? WhiteQueenImage : BlackQueenImage;
            RookBox.Image = isWhite ? WhiteRookImage : BlackRookImage;
            BishopBox.Image = isWhite ? WhiteBishopImage : BlackBishopImage;
            KnightBox.Image = isWhite ? WhiteKnightImage : BlackKnightImage;
            QueenBox.MouseEnter += PromotionPBox_MouseEnter;
            RookBox.MouseEnter += PromotionPBox_MouseEnter;
            BishopBox.MouseEnter += PromotionPBox_MouseEnter;
            KnightBox.MouseEnter += PromotionPBox_MouseEnter;
            QueenBox.MouseLeave += PromotionPBox_MouseLeave;
            RookBox.MouseLeave += PromotionPBox_MouseLeave;
            KnightBox.MouseLeave += PromotionPBox_MouseLeave;
            BishopBox.MouseLeave += PromotionPBox_MouseLeave;
            QueenBox.SizeMode = PictureBoxSizeMode.CenterImage;
            RookBox.SizeMode = PictureBoxSizeMode.CenterImage;
            BishopBox.SizeMode = PictureBoxSizeMode.CenterImage;
            KnightBox.SizeMode = PictureBoxSizeMode.CenterImage;
            QueenBox.Size = new System.Drawing.Size(60, 60);
            RookBox.Size = new System.Drawing.Size(60, 60);
            BishopBox.Size = new System.Drawing.Size(60, 60);
            KnightBox.Size = new System.Drawing.Size(60, 60);
            QueenBox.Tag = (isWhite ? "White" : "Black") + " Queen";
            RookBox.Tag = (isWhite ? "White" : "Black") + " Rook";
            BishopBox.Tag = (isWhite ? "White" : "Black") + " Bishop";
            KnightBox.Tag = (isWhite ? "White" : "Black") + " Knight";
            QueenBox.Click += PromoteBox_Click;
            RookBox.Click += PromoteBox_Click;
            BishopBox.Click += PromoteBox_Click;
            KnightBox.Click += PromoteBox_Click;
            QueenBox.Location = new Point(23, 5);
            RookBox.Location = new Point(23, QueenBox.Bottom + 3);
            BishopBox.Location = new Point(23, RookBox.Bottom + 3);
            KnightBox.Location = new Point(23, BishopBox.Bottom + 3);
            PromoteForm.Size = new Size(KnightBox.Width + 3, KnightBox.Bottom + 50);
            PromoteForm.Controls.Add(QueenBox);
            PromoteForm.Controls.Add(RookBox);
            PromoteForm.Controls.Add(BishopBox);
            PromoteForm.Controls.Add(KnightBox);
            PromoteForm.KeyDown += PromoteForm_KeyDown;
            Point location = new Point();
            location = Cursor.Position -
                new Size(PromoteForm.Width / 2, PromoteForm.Height / 2);
            if (location.X < this.Location.X)
                location.X = this.Location.X + 5;
            if (location.Y < this.Location.Y)
                location.Y = this.Location.Y + 5;
            if (location.X + PromoteForm.Width > this.Right)
                location.X = this.Right - PromoteForm.Width - 5;
            if (location.Y + PromoteForm.Height > this.Bottom)
                location.Y = this.Bottom - PromoteForm.Height - 5;
            PromoteForm.Location = location;
            PromoteForm.ShowDialog(this);
            PromoteForm_FormClosed(this, new FormClosedEventArgs(CloseReason.UserClosing));
        }
        void PromoteForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                PromoteForm.Close();
        }
        void PromotionPBox_MouseLeave(object sender, EventArgs e)
        {
            (sender as PictureBox).BackColor = Color.Transparent;
        }
        void PromotionPBox_MouseEnter(object sender, EventArgs e)
        {
            (sender as PictureBox).BackColor = Color.Orange;
        }
        private void PromoteForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (PromotePiece == null)
            {
                using (Graphics g = panel1.CreateGraphics())
                {
                    MoveCount--;
                    g.FillRectangle(PromoteSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, PromoteSquare.Rectangle);
                    if (PromoteSquare.Piece != null)
                        PlacePiece(PromoteSquare.Piece, PromoteSquare);
                    PlacePiece(PromotePawn, PromotePawn.Square);
                }
                return;
            }

            Position nextPosition = null;
            bool isPieceCaptured = false;
            Move move = new Move(PromotePawn.Square, PromotePawn, PromoteSquare);
            switch (PromotePiece.Type)
            {
                case Piece.PieceType.Queen:
                    move.PromoteType = Move.ItsPromoteType.Queen;
                    break;

                case Piece.PieceType.Rook:
                    move.PromoteType = Move.ItsPromoteType.Rook;
                    break;

                case Piece.PieceType.Bishop:
                    move.PromoteType = Move.ItsPromoteType.Bishop;
                    break;

                case Piece.PieceType.Knight:
                    move.PromoteType = Move.ItsPromoteType.Knight;
                    break;
            }
            if ((CurrentVariation.MovesList.IndexOf(CurrentPosition) <
                CurrentVariation.MovesList.Count - 1) && sender != PromotePiece)
            {
                nextPosition = CurrentVariation.MovesList
                    [CurrentVariation.MovesList.IndexOf(CurrentPosition) + 1];
                if (!move.Equals(nextPosition.LastMovePlayed))
                {
                    if (nextPosition.VariationHolders != null)
                        foreach (var item in nextPosition.VariationHolders)
                        {
                            if (item.MovesList[0].LastMovePlayed.Equals(move))
                            {
                                CurrentVariation = item;
                                using (Graphics g = panel1.CreateGraphics())
                                    g.FillRectangle(move.DestSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                                    LightSquareColor, move.DestSquare.Rectangle);
                                LoadPosition(item.MovesList[0], false);
                                UpdateNotationLabels();
                                return;
                            }
                        }

                    if (settings.behaviour.NewMoveHandling == "Always ask")   //Move is really new at this point
                    {
                        ShowNewVariationDialog();
                        if (newVarOption == NewVariationOption.Cancel)
                        {
                            MoveCount--;
                            panel1.CreateGraphics().FillRectangle(move.DestSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                                LightSquareColor, move.DestSquare.Rectangle);
                            PlacePiece(move.PieceMoving, move.OriginalSquare, false);
                            if (PromoteSquare.Piece != null)
                                PlacePiece(PromoteSquare.Piece, PromoteSquare);
                            newVarOption = NewVariationOption.NotDefined;
                            return;
                        }
                    }

                    else
                    {
                        if (settings.behaviour.NewMoveHandling == "Always new variation")
                            newVarOption = NewVariationOption.NewVariation;
                        if (settings.behaviour.NewMoveHandling == "Always overwrite")
                            newVarOption = NewVariationOption.Overwrite;
                        if (settings.behaviour.NewMoveHandling == "Always new main line")
                            newVarOption = NewVariationOption.NewMainLine;
                    }
                }
                else
                {
                    ReplayOldMove(move, panel1.CreateGraphics(), nextPosition);
                    return;
                }
            }

            using (Graphics g = panel1.CreateGraphics())
            {
                Pieces.Add(PromotePiece);
                if (PromoteSquare.Piece != null)
                {
                    Pieces.Remove(PromoteSquare.Piece);
                    isPieceCaptured = true;
                    CapturedPieces.Add(PromoteSquare.Piece);
                    PromoteSquare.Piece.Square = null;
                    PromoteSquare.Piece = null;
                }
                if (PromotePiece != sender)
                {
                    g.FillRectangle(PromoteSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, PromoteSquare.Rectangle);
                    g.FillRectangle(PromotePawn.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, PromotePawn.Square.Rectangle);
                }
                PlacePiece(PromotePiece, PromoteSquare, sender == PromotePiece);
                FiftyMoveCount = 0;
                CapturedPieces.Add(PromotePawn);
                Pieces.Remove(PromotePawn);
                sideToPlay = (sideToPlay == Piece.PieceSide.White) ? Piece.PieceSide.Black
                    : Piece.PieceSide.White;
                HandleChecking(sender == PromotePiece);
                if (sender != PromotePiece)
                    HandleDraws(new Move(PromotePawn.Square, PromotePawn, PromoteSquare));
                PromotePawn.Square.Piece = null;
                PromotePawn.Square = null;
                PromotePawn = null;
            }

            VariationHolder tempVariation = new VariationHolder();
            if (newVarOption == NewVariationOption.NewVariation)
            {
                if (nextPosition.VariationHolders == null)
                    nextPosition.VariationHolders = new List<VariationHolder>();
                VariationHolder tempVarHolder = new VariationHolder(new List<Position>(), CurrentVariation);
                tempVarHolder.ParentIndex = CurrentVariation.MovesList.IndexOf(CurrentPosition);
                tempVarHolder.ListOfParents = new List<VariationHolder>();

                VariationHolder CurrentNode = tempVarHolder.ParentLine;
                bool ShouldContinue = true;
                while (ShouldContinue)
                {
                    tempVarHolder.ListOfParents.Add(CurrentNode);
                    if (CurrentNode.ParentLine != null)
                        CurrentNode = CurrentNode.ParentLine;
                    else ShouldContinue = false;
                }

                CurrentVariation = tempVarHolder;
                nextPosition.VariationHolders.Add(tempVarHolder);
                GameVariations.Add(tempVarHolder);
            }

            else if (newVarOption == NewVariationOption.NewMainLine)
            {
                InsertNewMove(nextPosition, true, out tempVariation);
            }
            else if (newVarOption == NewVariationOption.Overwrite)
            {
                InsertNewMove(nextPosition, false, out tempVariation);
            }
            move.IsCapture = isPieceCaptured;
            move.GetShortNotation();
            move.ShortNotation += ((!(IsWhiteCheckmated || IsBlackCheckmated) && (IsWhiteInCheck || IsBlackInCheck)) ? "+" : "") +
                ((IsWhiteCheckmated || IsBlackCheckmated) ? "#" : "");
            SavePosition(move, sender == PromotePiece);
            PromotePiece = null;

            if (tempVariation != null && newVarOption == NewVariationOption.NewMainLine)
            {
                CurrentPosition.VariationHolders = new List<VariationHolder>();
                CurrentPosition.VariationHolders.Add(tempVariation);
            }
            newVarOption = NewVariationOption.NotDefined;
        }
        private void PromoteBox_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if ((string)pb.Tag == "White Queen")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.White;
                PromotePiece.Type = Piece.PieceType.Queen;
                PromotePiece.Image = WhiteQueenImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "Black Queen")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.Black;
                PromotePiece.Type = Piece.PieceType.Queen;
                PromotePiece.Image = BlackQueenImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "White Rook")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.White;
                PromotePiece.Type = Piece.PieceType.Rook;
                PromotePiece.Image = WhiteRookImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "Black Rook")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.Black;
                PromotePiece.Type = Piece.PieceType.Rook;
                PromotePiece.Image = BlackRookImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "White Bishop")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.White;
                PromotePiece.Type = Piece.PieceType.Bishop;
                PromotePiece.Image = WhiteBishopImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "Black Bishop")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.Black;
                PromotePiece.Type = Piece.PieceType.Bishop;
                PromotePiece.Image = BlackBishopImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "White Knight")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.White;
                PromotePiece.Type = Piece.PieceType.Knight;
                PromotePiece.Image = WhiteKnightImage;
                PromoteForm.Close();
            }
            if ((string)pb.Tag == "Black Knight")
            {
                PromotePiece = new Piece();
                PromotePiece.Side = Piece.PieceSide.Black;
                PromotePiece.Type = Piece.PieceType.Knight;
                PromotePiece.Image = BlackKnightImage;
                PromoteForm.Close();
            }
        }
        private bool HandleCastling(Piece piece, Square square, Graphics g, bool isPlayOut, Form1 form1)
        {
            if (piece.Square.Name == "a1")
                QueensideCastlingWhite = false;
            if (piece.Square.Name == "h1")
                KingsideCastlingWhite = false;
            if (piece.Square.Name == "a8")
                QueensideCastlingBlack = false;
            if (piece.Square.Name == "h8")
                KingsideCastlingBlack = false;

            if (piece.Type == Piece.PieceType.King)
            {
                if (piece.Side == Piece.PieceSide.White)
                {
                    QueensideCastlingWhite = false;
                    KingsideCastlingWhite = false;
                }
                else
                {
                    QueensideCastlingBlack = false;
                    KingsideCastlingBlack = false;
                }
            }
            if (piece == WhiteKing && piece.Square == GetSquare("e1") && square == GetSquare("g1"))
            {
                piece.Square.Piece = null;
                if (!isPlayOut)
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, square.Rectangle);
                    g.FillRectangle(piece.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, piece.Square.Rectangle);
                    g.FillRectangle(GetSquare("h1").Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, GetSquare("h1").Rectangle);
                }
                PlacePiece(piece, square, isPlayOut);
                PlacePiece(GetSquare("h1").Piece, GetSquare("f1"), isPlayOut);
                return true;
            }

            else if (piece == WhiteKing && piece.Square == GetSquare("e1") && square == GetSquare("c1"))
            {
                piece.Square.Piece = null;
                if (!isPlayOut)
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, square.Rectangle);
                    g.FillRectangle(piece.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, piece.Square.Rectangle);
                    g.FillRectangle(GetSquare("a1").Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, GetSquare("a1").Rectangle);
                }
                PlacePiece(piece, square, isPlayOut);
                PlacePiece(GetSquare("a1").Piece, GetSquare("d1"), isPlayOut);
                return true;
            }

            else if (piece == BlackKing && piece.Square == GetSquare("e8") && square == GetSquare("g8"))
            {
                piece.Square.Piece = null;
                if (!isPlayOut)
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, square.Rectangle);
                    g.FillRectangle(piece.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, piece.Square.Rectangle);
                    g.FillRectangle(GetSquare("h8").Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, GetSquare("h8").Rectangle);
                }
                PlacePiece(piece, square, isPlayOut);
                PlacePiece(GetSquare("h8").Piece, GetSquare("f8"), isPlayOut);
                return true;
            }

            else if (piece == BlackKing && piece.Square == GetSquare("e8") && square == GetSquare("c8"))
            {
                piece.Square.Piece = null;
                if (!isPlayOut)
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, square.Rectangle);
                    g.FillRectangle(piece.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, piece.Square.Rectangle);
                    g.FillRectangle(GetSquare("a8").Type == Square.SquareType.Dark ? DarkSquareColor :
                        LightSquareColor, GetSquare("a8").Rectangle);
                }
                PlacePiece(piece, square, isPlayOut);
                PlacePiece(GetSquare("a8").Piece, GetSquare("d8"), isPlayOut);
                return true;
            }
            else return false;
        }
        private void AttemptBlocking()
        {
            foreach (Piece piece in Pieces)
            {
                if (piece.Type == Piece.PieceType.King || piece.Side == CheckingPiece.Side)
                    continue;
                ValidSquares = GetValidSquares(piece);
                PieceCrossCheck(piece);
                foreach (Square item in PossibleBlockingSquares)
                {
                    if (ValidSquares.Contains(item))
                        PossibleDefences.Add(new Move(piece, item));
                }
            }
        }
        private void AttemptCapturingChecker()
        {
            foreach (Piece piece in Pieces)
            {
                if (piece.Type == Piece.PieceType.King)
                    continue;
                if ((IsWhiteInCheck && piece.Side == Piece.PieceSide.White) ||
                    (IsBlackInCheck && piece.Side == Piece.PieceSide.Black))
                {
                    ValidSquares = GetValidSquares(piece);
                    PieceCrossCheck(piece);
                    foreach (Square Sq in ValidSquares)
                    {
                        if (Sq.Piece != null && Sq.Piece == CheckingPiece)
                            PossibleDefences.Add(new Move(piece, Sq));
                    }
                }
            }

        }
        private List<Square> GetBlockingSquares(Piece king, Piece piece)
        {
            List<Square> tempSquares = new List<Square>();
            int mainKInt, mainPInt, tempKInt, tempPInt;
            char mainKChar, mainPChar, tempKChar, tempPChar;
            mainPChar = (char)piece.Square.Name[0];
            mainPInt = int.Parse(piece.Square.Name.Substring(1, 1));
            tempPChar = mainPChar;
            tempPInt = mainPInt;

            mainKChar = (char)king.Square.Name[0];
            mainKInt = int.Parse(king.Square.Name.Substring(1, 1));

            tempKChar = mainKChar;
            tempKInt = mainKInt;

            switch (piece.Type)
            {
                case Piece.PieceType.Queen:

                    if (mainPChar == mainKChar || mainPInt == mainKInt)
                    {
                        // Presume lateral checking
                        if (mainKInt - mainPInt > 1)
                        {
                            for (int i = 0; i < mainKInt - mainPInt - 1; i++)
                            {
                                tempPInt++;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPInt = mainPInt;
                        if (mainPInt - mainKInt > 1)
                        {
                            for (int i = 0; i < mainPInt - mainKInt - 1; i++)
                            {
                                tempKInt++;
                                tempSquares.Add(GetSquare(tempKChar.ToString() + tempKInt.ToString()));
                            }
                        }

                        tempKInt = mainKInt;
                        if (mainKChar - mainPChar > 1)
                        {
                            for (int i = 0; i < mainKChar - mainPChar - 1; i++)
                            {
                                tempPChar++;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPChar = mainPChar;
                        if (mainPChar - mainKChar > 1)
                        {
                            for (int i = 0; i < mainPChar - mainKChar - 1; i++)
                            {
                                tempKChar++;
                                tempSquares.Add(GetSquare(tempKChar.ToString() + tempKInt.ToString()));
                            }
                        }
                    }
                    else
                    {
                        //presume diagonal checking
                        if (mainKChar - mainPChar > 1 && mainKInt > mainPInt
                        && mainKChar - mainPChar == mainKInt - mainPInt) // Queen is lower left
                        {
                            for (int i = 0; i < mainKChar - mainPChar - 1; i++)
                            {
                                tempPChar++; tempPInt++;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPInt = mainPInt; tempPChar = mainPChar;
                        if (mainKChar - mainPChar > 1 && mainPInt > mainKInt
                        && mainKChar - mainPChar == mainPInt - mainKInt) // Queen is upper left
                        {
                            for (int i = 0; i < mainKChar - mainPChar - 1; i++)
                            {
                                tempPChar++; tempPInt--;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPInt = mainPInt; tempPChar = mainPChar;
                        if (mainPChar - mainKChar > 1 && mainPInt > mainKInt &&
                            mainPChar - mainKChar == mainPInt - mainKInt) // Queen is upper right
                        {
                            for (int i = 0; i < mainPChar - mainKChar - 1; i++)
                            {
                                tempPChar--; tempPInt--;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPInt = mainPInt; tempPChar = mainPChar;
                        if (mainPChar - mainKChar > 1 && mainKInt > mainPInt
                        && mainPChar - mainKChar == mainKInt - mainPInt) // Queen is lower right
                        {
                            for (int i = 0; i < mainPChar - mainKChar - 1; i++)
                            {
                                tempPChar--; tempPInt++;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }
                    }
                    break;

                case Piece.PieceType.Rook:

                    if (mainPChar == mainKChar || mainPInt == mainKInt)
                    {
                        if (mainKInt - mainPInt > 1)
                        {
                            for (int i = 0; i < mainKInt - mainPInt - 1; i++)
                            {
                                tempPInt++;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPInt = mainPInt;
                        if (mainPInt - mainKInt > 1)
                        {
                            for (int i = 0; i < mainPInt - mainKInt - 1; i++)
                            {
                                tempKInt++;
                                tempSquares.Add(GetSquare(tempKChar.ToString() + tempKInt.ToString()));
                            }
                        }

                        tempKInt = mainKInt;
                        if (mainKChar - mainPChar > 1)
                        {
                            for (int i = 0; i < mainKChar - mainPChar - 1; i++)
                            {
                                tempPChar++;
                                tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                            }
                        }

                        tempPChar = mainPChar;
                        if (mainPChar - mainKChar > 1)
                        {
                            for (int i = 0; i < mainPChar - mainKChar - 1; i++)
                            {
                                tempKChar++;
                                tempSquares.Add(GetSquare(tempKChar.ToString() + tempKInt.ToString()));
                            }
                        }
                    }
                    break;

                case Piece.PieceType.Bishop:

                    if (mainKChar - mainPChar > 1 && mainKInt > mainPInt
                        && mainKChar - mainPChar == mainKInt - mainPInt) // Bishop is lower left
                    {
                        for (int i = 0; i < mainKChar - mainPChar - 1; i++)
                        {
                            tempPChar++; tempPInt++;
                            tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                        }
                    }

                    tempPInt = mainPInt; tempPChar = mainPChar;
                    if (mainKChar - mainPChar > 1 && mainPInt > mainKInt
                        && mainKChar - mainPChar == mainPInt - mainKInt) // Bishop is upper left
                    {
                        for (int i = 0; i < mainKChar - mainPChar - 1; i++)
                        {
                            tempPChar++; tempPInt--;
                            tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                        }
                    }

                    tempPInt = mainPInt; tempPChar = mainPChar;
                    if (mainPChar - mainKChar > 1 && mainPInt > mainKInt
                        && mainPChar - mainKChar == mainPInt - mainKInt) // Bishop is upper right
                    {
                        for (int i = 0; i < mainPChar - mainKChar - 1; i++)
                        {
                            tempPChar--; tempPInt--;
                            tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                        }
                    }

                    tempPInt = mainPInt; tempPChar = mainPChar;
                    if (mainPChar - mainKChar > 1 && mainKInt > mainPInt
                        && mainPChar - mainKChar == mainKInt - mainPInt) // Bishop is lower right
                    {
                        for (int i = 0; i < mainPChar - mainKChar - 1; i++)
                        {
                            tempPChar--; tempPInt++;
                            tempSquares.Add(GetSquare(tempPChar.ToString() + tempPInt.ToString()));
                        }
                    }
                    break;
            }

            return tempSquares;
        }
        private bool CheckMove(Move move)
        {
            if (move.DestSquare == move.PieceMoving.Square)
                return false;
            if (move.PieceMoving.Side != sideToPlay)
                return false;

            if (IsWhiteInCheck || IsBlackInCheck)
            {
                if (PossibleDefences.Contains(move))
                    return true;
                else
                    return false;
            }

            ValidSquares = GetValidSquares(move.PieceMoving);

            if (move.PieceMoving.Type == Piece.PieceType.King)
            {
                KingCrossCheck(move.PieceMoving);
            }
            else
                PieceCrossCheckEncore(move.PieceMoving);

            if (ValidSquares.Contains(move.DestSquare))
                return true;
            else
                return false;
        }
        private void PieceCrossCheck(Piece piece)
        {
            if (piece.Type != Piece.PieceType.King)
            {
                bool isWhite = (piece.Side == Piece.PieceSide.White) ? true : false;
                foreach (Piece attacker in Pieces)
                {
                    if ((attacker.Side != piece.Side) && piece != CheckingPiece)
                    {
                        if (attacker.Type == Piece.PieceType.Bishop || attacker.Type == Piece.PieceType.Rook
                            || attacker.Type == Piece.PieceType.Queen)
                        {
                            List<Square> tempSq = GetValidSquares(attacker);
                            if (tempSq.Contains(piece.Square))
                            {
                                Piece tempPiece = piece;
                                piece.Square.Piece = null;
                                tempSq = GetValidSquares(attacker);
                                if ((isWhite && tempSq.Contains(WhiteKing.Square)) ||
                                    (!isWhite && tempSq.Contains(BlackKing.Square)))
                                {
                                    tempSq = GetBlockingSquares((isWhite ? WhiteKing : BlackKing),
                                        attacker);
                                    tempSq.Add(attacker.Square);   // includes option of capturing attacker
                                    List<Square> tempSq2 = new List<Square>();
                                    foreach (var item in tempSq)
                                        if (ValidSquares.Contains(item))
                                            tempSq2.Add(item);
                                    ValidSquares = tempSq2;
                                }

                                tempPiece.Square.Piece = tempPiece;
                            }
                        }
                    }
                }
            }
        }
        private void PieceCrossCheckEncore(Piece piece)
        {
            PieceMoving = piece;
            Parallel.ForEach(Pieces, PCC);
        }
        private void PCC(Piece attacker)
        {
            if (attacker.Side != PieceMoving.Side &&
                (attacker.Type == Piece.PieceType.Bishop || attacker.Type == Piece.PieceType.Rook
                || attacker.Type == Piece.PieceType.Queen))
            {
                List<Square> TempList = GetValidSquares(attacker);
                if (TempList.Contains(PieceMoving.Square))
                {
                    TempList = GetBlockingSquares(PieceMoving.Side == Piece.PieceSide.Black
                        ? BlackKing : WhiteKing, attacker);
                    bool isPinned = true;
                    foreach (var item in TempList)
                    {
                        if (item.Piece != null && item.Piece != PieceMoving)
                            isPinned = false;
                    }
                    if (isPinned && TempList.Count > 0)
                    {
                        List<Square> tempList2 = new List<Square>();
                        TempList.Add(attacker.Square);  //Includes option of capturing attacker
                        lock (locker)
                        {
                            foreach (var item in TempList)
                            {
                                if (ValidSquares.Contains(item))
                                    tempList2.Add(item);
                            }
                            ValidSquares = tempList2;
                        }
                    }
                }
            }
        }
        private void KingCrossCheck(Piece piece)
        {
            if (piece.Type != Piece.PieceType.King)
                return;

            List<Square> unsafeSq = new List<Square>();
            tester = true;
            foreach (Piece item in Pieces)
            {
                if (item.Side != piece.Side)
                {
                    unsafeSq = GetValidSquares(item);
                    foreach (Square sq in unsafeSq)
                        if (ValidSquares.Contains(sq))
                            ValidSquares.Remove(sq);
                    unsafeSq.Clear();
                }
            }
            tester = false;
        }
        static public List<Square> GetValidSquares(Engine engine, Piece piece)
        {
            Square square = piece.Square;

            if (engine != null)
                foreach (var item in engine.PVSquares)
                    if (piece == item.Piece)
                    {
                        square = item;
                        break;
                    }

            List<Square> tempSquares = new List<Square>();
            Square tempSquare;
            char mainChar = (char)square.Name[0];
            char tempChar = mainChar;
            String tempStr = "", str = square.Name.Substring(1, 1);
            int mainInt = int.Parse(str);
            int tempInt = mainInt;

            switch (piece.Type)
            {

                case Piece.PieceType.King:

                    if (piece.Side == Piece.PieceSide.White && !IsWhiteInCheck && !tester &&
                        (KingsideCastlingWhite || QueensideCastlingWhite))
                    {
                        tester = true;
                        bool KSfail = KingsideCastlingWhite ? false : true,
                            QSfail = QueensideCastlingWhite ? false : true;
                        if (GetSquare("b1").Piece != null || GetSquare("c1").Piece != null ||
                            GetSquare("d1").Piece != null)
                            QSfail = true;
                        if (GetSquare("f1").Piece != null || GetSquare("g1").Piece != null)
                            KSfail = true;
                        List<Square> tempSq;
                        foreach (Piece item in Pieces)
                        {
                            if (KSfail && QSfail)
                                break;
                            if (piece != item && piece.Side != item.Side)
                            {
                                tempSq = GetValidSquares(item);
                                if (tempSq.Contains(GetSquare("f1")) || tempSq.Contains(GetSquare("g1")))
                                    KSfail = true;
                                if (tempSq.Contains(GetSquare("d1")) || tempSq.Contains(GetSquare("c1")))
                                    QSfail = true;
                            }
                        }
                        if (!KSfail && WhiteKing.Square == GetSquare("e1"))
                            tempSquares.Add(GetSquare("g1"));
                        if (!QSfail && WhiteKing.Square == GetSquare("e1"))
                            tempSquares.Add(GetSquare("c1"));
                        tester = false;
                    }

                    else if (piece.Side == Piece.PieceSide.Black && !IsBlackInCheck && !tester &&
                        (KingsideCastlingBlack || QueensideCastlingBlack))
                    {
                        tester = true;
                        bool KSfail = KingsideCastlingBlack ? false : true,
                            QSfail = QueensideCastlingBlack ? false : true;
                        if (GetSquare("b8").Piece != null || GetSquare("c8").Piece != null ||
                            GetSquare("d8").Piece != null)
                            QSfail = true;
                        if (GetSquare("f8").Piece != null || GetSquare("g8").Piece != null)
                            KSfail = true;
                        List<Square> tempSq;
                        foreach (Piece item in Pieces)
                        {
                            if (KSfail && QSfail)
                                break;
                            if (piece != item && piece.Side != item.Side)
                            {
                                tempSq = GetValidSquares(item);
                                if (tempSq.Contains(GetSquare("f8")) || tempSq.Contains(GetSquare("g8")))
                                    KSfail = true;
                                if (tempSq.Contains(GetSquare("d8")) || tempSq.Contains(GetSquare("c8")))
                                    QSfail = true;
                            }
                        }
                        if (!KSfail && BlackKing.Square == GetSquare("e8"))
                            tempSquares.Add(GetSquare("g8"));
                        if (!QSfail && BlackKing.Square == GetSquare("e8"))
                            tempSquares.Add(GetSquare("c8"));
                        tester = false;
                    }

                    tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt -= 2; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt++; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);

                    }
                    tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt -= 2; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt++; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempInt -= 2; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    break;

                case Piece.PieceType.Queen:

                    // Diagonal starts
                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar++; tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar--; tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar--; tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar++; tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    // Lateral starts
                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }
                    break;

                case Piece.PieceType.Rook:

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null)
                            {
                                if (tester)
                                {
                                    if (tempSquare.Piece.Type == Piece.PieceType.King
                                        && tempSquare.Piece.Side != piece.Side)
                                        continue;
                                }
                                break;
                            }
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null)
                            {
                                if (tester)
                                {
                                    if (tempSquare.Piece.Type == Piece.PieceType.King
                                        && tempSquare.Piece.Side != piece.Side)
                                        continue;
                                }
                                break;
                            }
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null)
                            {
                                if (tester)
                                {
                                    if (tempSquare.Piece.Type == Piece.PieceType.King
                                        && tempSquare.Piece.Side != piece.Side)
                                        continue;
                                }
                                break;
                            }
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null)
                            {
                                if (tester)
                                {
                                    if (tempSquare.Piece.Type == Piece.PieceType.King
                                        && tempSquare.Piece.Side != piece.Side)
                                        continue;
                                }
                                break;
                            }
                        }
                        else break;
                    }
                    break;

                case Piece.PieceType.Bishop:

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar++; tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar--; tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar--; tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    for (int i = 0; i < 7; i++)
                    {
                        tempChar++; tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if (tester)
                                tempSquares.Add(tempSquare);
                            else if (tempSquare.Piece == null || (tempSquare.Piece != null
                            && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                            else break;
                            if (tempSquare.Piece != null && !(tester
                                && tempSquare.Piece.Type == Piece.PieceType.King
                                && tempSquare.Piece.Side != piece.Side)) break;
                        }
                        else break;
                    }
                    break;

                case Piece.PieceType.Knight:

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt += 2; tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt += 2; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt++; tempChar++; tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt--; tempChar++; tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt++; tempChar--; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt--; tempChar--; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt -= 2; tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }

                    tempChar = mainChar; tempInt = mainInt;
                    tempInt -= 2; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                    if (TryGetSquare(engine, tempStr, out tempSquare))
                    {
                        if (tester)
                            tempSquares.Add(tempSquare);
                        else if (tempSquare.Piece == null || (tempSquare.Piece != null
                        && tempSquare.Piece.Side != piece.Side)) tempSquares.Add(tempSquare);
                    }
                    break;

                case Piece.PieceType.Pawn:

                    if (!tester && EnPassantPawn != null && piece.Side != EnPassantPawn.Side)
                    {
                        if (int.Parse(piece.Square.Name.Substring(1, 1)) == 
                            int.Parse(EnPassantPawn.Square.Name.Substring(1, 1)))
                        {
                            if (Math.Abs(piece.Square.Name[0] - EnPassantPawn.Square.Name[0]) == 1)
                                tempSquares.Add(GetSquare(EnPassantPawn.Square.Name[0].ToString()
                                    + (int.Parse(piece.Square.Name.Substring(1, 1)) +
                                    (piece.Side == Piece.PieceSide.White ? 1 : -1)).ToString()));
                        }

                    }

                    if (piece.Side == Piece.PieceSide.White)
                    {
                        tempChar = mainChar; tempInt = mainInt;
                        for (int i = 0; i < 2; i++)
                        {
                            if (i == 1 && mainInt != 2)
                                break;

                            tempInt++; tempStr = tempChar.ToString() + tempInt.ToString();
                            if (TryGetSquare(engine, tempStr, out tempSquare))
                            {
                                if (tempSquare.Piece == null && !tester) tempSquares.Add(tempSquare);
                                else break;
                            }
                        }

                        tempChar = mainChar; tempInt = mainInt;
                        tempInt++; tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if ((tempSquare.Piece != null &&
                                tempSquare.Piece.Side != piece.Side) || tester)
                            {
                                tempSquares.Add(tempSquare);
                            }
                        }

                        tempChar = mainChar; tempInt = mainInt;
                        tempInt++; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if ((tempSquare.Piece != null &&
                                tempSquare.Piece.Side != piece.Side) || tester)
                            {
                                tempSquares.Add(tempSquare);
                            }
                        }
                    }

                    else
                    {
                        tempChar = mainChar; tempInt = mainInt;
                        for (int i = 0; i < 2; i++)
                        {
                            if (i == 1 && mainInt != 7)
                                break;
                            tempInt--; tempStr = tempChar.ToString() + tempInt.ToString();
                            if (TryGetSquare(engine, tempStr, out tempSquare))
                            {
                                if (tempSquare.Piece == null && !tester) tempSquares.Add(tempSquare);
                                else break;
                            }
                        }

                        tempChar = mainChar; tempInt = mainInt;
                        tempInt--; tempChar--; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if ((tempSquare.Piece != null &&
                                tempSquare.Piece.Side != piece.Side) || tester)
                            {
                                tempSquares.Add(tempSquare);
                            }
                        }

                        tempChar = mainChar; tempInt = mainInt;
                        tempInt--; tempChar++; tempStr = tempChar.ToString() + tempInt.ToString();
                        if (TryGetSquare(engine, tempStr, out tempSquare))
                        {
                            if ((tempSquare.Piece != null &&
                                tempSquare.Piece.Side != piece.Side) || tester)
                            {
                                tempSquares.Add(tempSquare);
                            }
                        }
                    }
                    break;
            }
            return tempSquares;
        }
        static public List<Square> GetValidSquares(Piece piece)
        {
            return GetValidSquares(null, piece);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (state == null)
                state = new State();
            state.Game = new Game();
            state.Game.CapturedPieces = CapturedPieces;
            state.Game.CurrentPosition = CurrentPosition;
            state.Game.CurrentVariation = CurrentVariation;
            state.Game.GameDetails = gameDetails;
            state.Game.GameVariations = GameVariations;
            state.Game.MainLine = MainLine;
            state.Game.Pieces = Pieces;
            state.Squares = Squares;
            state.TwoDSquares = TwoDSquares;
            state.FENForStartPosition = FENforStartPos;
            state.StartingPosition = StartingPosition;
            state.isBoardFlipped = isBoardFlipped;

            state.BlackTime = BlackTime;
            state.CurrentPlayer = CurrentPlayer;
            state.CurrentUser = CurrentUser;
            state.FirstEngine = FirstEngine;
            state.SecondEngine = SecondEngine;
            state.ShouldClockTick = ShouldClockTick;
            state.TimeControl = timeControl;
            state.UserList = UserList;
            state.WhiteTime = WhiteTime;
            state.isEngineMatchInProgress = isEngineMatchInProgress;
            state.isRatedGameInProgress = isRatedGameInProgress;
            state.ModeOfPlay = ModeOfPlay;

            BinaryFormatter bf = new BinaryFormatter();
            Directory.CreateDirectory("Engines");
            using (Stream output = File.Create(@"Engines\Installed Engines.dat"))
            {
                bf.Serialize(output, InstalledEngines);
            }

            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");
            using (Stream output = File.Create(@"Data\state.dat"))
            {
                bf.Serialize(output, state);
            }

            using (Stream output = File.Create(@"Data\settings.dat"))
            {
                bf.Serialize(output, settings);
            }
            //using (Stream output = File.Create("Engines//OpeningBook.dat"))
            //{
            //    bf.Serialize(output, TabierList);
            //}
        }
        private void panel1_Resize(object sender, EventArgs e)
        {
            if (Squares == null)
                return;
            InitializeSquareCordinates();
        }
        private void gamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (Squares == null)
                return;
            InitializeSquareCordinates();
        }
        [Serializable]
        public class Square
        {
            [Serializable]
            public enum SquareType
            {
                Dark,
                Light
            }
            public Rectangle Rectangle { get; set; }
            public String Name { get; set; }
            public Piece Piece { get; set; }
            public SquareType Type { get; set; }
            public Square GetClone()
            {
                Square square = new Square();
                square.Rectangle = this.Rectangle;
                square.Piece = this.Piece;
                square.Name = this.Name;
                square.Type = this.Type;
                return square;
            }
            public Square FlipSquare { get; set; }
            public Rectangle EDRectangle { get; set; }
            public Piece EDPiece { get; set; }
            public override string ToString()
            {
                return Name;
            }
            public bool Compare(Square other)
            {
                return this.Name.CompareTo(other.Name) == 0;
            }
        }
        [Serializable]
        public class Piece
        {
            public Piece()
            {

            }
            public Piece(String name, PieceType type, PieceSide side, Bitmap image)
            {
                Name = name;
                Type = type;
                Side = side;
                Image = image;
            }
            public Piece(PieceSide side, PieceType type)
            {
                Side = side;
                Type = type;
                switch (Type)
                {
                    case PieceType.King:
                        Name = Side == PieceSide.White ? "White King" : "Black King";
                        Image = Side == PieceSide.White ? WhiteKingImage : BlackKingImage;
                        break;
                    case PieceType.Queen:
                        Name = Side == PieceSide.White ? "White Queen" : "Black Queen";
                        Image = Side == PieceSide.White ? WhiteQueenImage : BlackQueenImage;
                        break;
                    case PieceType.Rook:
                        Name = Side == PieceSide.White ? "White Rook" : "Black Rook";
                        Image = Side == PieceSide.White ? WhiteRookImage : BlackRookImage;
                        break;
                    case PieceType.Bishop:
                        Name = Side == PieceSide.White ? "White Bishop" : "Black Bishop";
                        Image = Side == PieceSide.White ? WhiteBishopImage : BlackBishopImage;
                        break;
                    case PieceType.Knight:
                        Name = Side == PieceSide.White ? "White Knight" : "Black Knight";
                        Image = Side == PieceSide.White ? WhiteKnightImage : BlackKnightImage;
                        break;
                    case PieceType.Pawn:
                        Name = Side == PieceSide.White ? "White Pawn" : "Black Pawn";
                        Image = Side == PieceSide.White ? WhitePawnImage : BlackPawnImage;
                        break;
                }
            }
            [Serializable]
            public enum PieceType
            {
                King,
                Queen,
                Rook,
                Bishop,
                Knight,
                Pawn
            }
            [Serializable]
            public enum PieceSide
            {
                White,
                Black
            }
            public PieceSide Side { get; set; }
            public PieceType Type { get; set; }
            public Square Square { get; set; }
            public String Name { get; set; }
            public Bitmap Image { get; set; }
            public Point Location { get; set; }
            public override string ToString()
            {
                return Name + (Square != null ? " on " +Square.Name : "");
            }
            public bool Compare(Piece other)
            {
                return this.Type == other.Type && this.Side == other.Side;
            }
        }
        [Serializable]
        new public class Move : IEquatable<Move>
        {
            [Serializable]
            public enum ItsPromoteType
            {
                Queen,
                Rook,
                Bishop,
                Knight,
                NotAvailable
            }
            public Move()
            {
                PromoteType = ItsPromoteType.NotAvailable;
                IsCapture = false;
                Comment = new Comment();
            }
            public Move(Square square1, Piece piece, Square square2)
            {
                OriginalSquare = square1;
                PieceMoving = piece;
                DestSquare = square2;
                Name = this.ToString();
                PromoteType = ItsPromoteType.NotAvailable;
                IsCapture = false;
                Comment = new Comment();
            }
            public Move(Piece piece, Square square)
            {
                OriginalSquare = piece.Square;
                PieceMoving = piece;
                DestSquare = square;
                Name = this.ToString();
                PromoteType = ItsPromoteType.NotAvailable;
                IsCapture = false;
                Comment = new Comment();
            }
            public Move(Square square1, Piece piece, Square square2, ItsPromoteType promoteType, bool isPieceCaptured)
            {
                OriginalSquare = square1;
                PieceMoving = piece;
                DestSquare = square2;
                Name = this.ToString();
                PromoteType = promoteType;
                IsCapture = isPieceCaptured;
                Comment = new Comment();
            }
            public void GetShortNotation()
            {
                GetShortNotation(null);
            }
            public void GetShortNotation(Engine engine)
            {
                UCINotation = OriginalSquare.Name + DestSquare.Name;

                String tempStr = "";
                switch (this.PieceMoving.Type)
                {
                    case Piece.PieceType.King:
                        if ((OriginalSquare.Name[0] - DestSquare.Name[0]) == 2)
                            tempStr = "O-O-O";
                        else if ((OriginalSquare.Name[0] - DestSquare.Name[0]) == -2)
                            tempStr = "O-O";
                        else
                        {
                            tempStr += "K";
                            if (IsCapture)
                                tempStr += "x";
                            tempStr += DestSquare.Name;
                        }
                        break;
                    case Piece.PieceType.Queen:
                        tempStr += "Q";
                        break;
                    case Piece.PieceType.Rook:
                        tempStr += "R";
                        break;
                    case Piece.PieceType.Bishop:
                        tempStr += "B";
                        break;
                    case Piece.PieceType.Knight:
                        tempStr += "N";
                        break;
                    case Piece.PieceType.Pawn:
                        if (int.Parse(DestSquare.Name.Substring(1, 1)) == 8 ||
                            int.Parse(DestSquare.Name.Substring(1, 1)) == 1)
                        {
                            tempStr += (IsCapture ? (OriginalSquare.Name[0].ToString() + "x") : "")
                                + DestSquare.Name;
                            switch (PromoteType)
                            {
                                case ItsPromoteType.Queen:
                                    tempStr += "Q";
                                    UCINotation += "q";
                                    break;
                                case ItsPromoteType.Rook:
                                    tempStr += "R";
                                    UCINotation += "r";
                                    break;
                                case ItsPromoteType.Bishop:
                                    tempStr += "B";
                                    UCINotation += "b";
                                    break;
                                case ItsPromoteType.Knight:
                                    tempStr += "N";
                                    UCINotation += "n";
                                    break;
                                case ItsPromoteType.NotAvailable:
                                    MessageBox.Show("ERROR!, PromoteType is N/A!!");
                                    break;
                            }
                        }
                        else
                        {
                            if (IsCapture)
                                tempStr += OriginalSquare.Name[0].ToString() + "x";
                            tempStr += DestSquare.Name;
                        }
                        break;
                }
                if (PieceMoving.Type != Piece.PieceType.Pawn &&
                    PieceMoving.Type != Piece.PieceType.King)
                {
                    List<Piece> tempList = new List<Piece>();
                    List<Square> tempSquares = new List<Square>();
                    foreach (var item in (engine == null ? Squares : engine.PVSquares))
                        if (item.Piece != null && item.Piece.Side == PieceMoving.Side && 
                            item.Piece.Type == PieceMoving.Type  && item.Piece != PieceMoving)
                        {
                            bool t = tester;
                            tester = false;
                            tempSquares = GetValidSquares(engine, item.Piece);
                            tester = t;
                            foreach (var square in tempSquares)
                                if (square.Name == DestSquare.Name)
                                {
                                    tempList.Add(item.Piece);
                                    break;
                                }
                        }

                    bool freeRank = true;
                    bool freeFile = true;
                    foreach (var item in tempList)
                    {
                        if (item.Square.Name[0] == OriginalSquare.Name[0])
                            freeFile = false;
                        if (item.Square.Name.Substring(1, 1) == OriginalSquare.Name.Substring(1, 1))
                            freeRank = false;
                    }
                    if (tempList.Count > 0 && freeFile)
                        tempStr += OriginalSquare.Name[0].ToString();
                    if (tempList.Count > 0 && !freeFile && freeRank)
                        tempStr += OriginalSquare.Name.Substring(1, 1);
                    if (IsCapture)
                        tempStr += "x";
                    tempStr += DestSquare.Name;
                }
                ShortNotation = tempStr;
            }
            public String UCINotation { get; set; }
            public String Name { get; set; }
            public String ShortNotation { get; set; }
            public Comment Comment { get; set; }
            public bool IsCapture { get; set; }
            public Square DestSquare { get; set; }
            public Piece PieceMoving { get; set; }
            public Square OriginalSquare { get; set; }
            public int MoveNo { get; set; }
            public ItsPromoteType PromoteType { get; set; }
            public override string ToString()
            {
                return PieceMoving.Name + " on " + OriginalSquare.Name + ((IsCapture) ? " takes on " : " goes on ")
                    + DestSquare.Name;
            }
            public bool Equals(Move other)
            {
                if (this.PieceMoving == other.PieceMoving && this.DestSquare == other.DestSquare &&
                    this.OriginalSquare == other.OriginalSquare && this.PromoteType == other.PromoteType)
                    return true;
                return false;
            }
            public override bool Equals(object obj)
            {
                if (obj is Move)
                    return Equals(obj as Move);
                return false;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();          // LOOK UP GETTING HASHCODES FOR PROPERTIES
            }
        }
        [Serializable]
        public class GameDetails
        {
            [Serializable]
            public enum Outcome
            {
                WhiteWin,
                Draw,
                BlackWin,
                NotAvailable
            }
            public bool isGameRated { get; set; }
            public OpeningNode FinalTabier { get; set; }
            public String GetResultString()
            {
                switch (Result)
                {
                    case Outcome.WhiteWin:
                        return "1-0";
                    case Outcome.Draw:
                        return "1/2";
                    case Outcome.BlackWin:
                        return "0-1";
                    default:
                        return "";
                }
            }
            public bool isUserGame { get; set; }
            public String RegularDateString { get; set; }
            public IPlayer WhitePlayer { get; set; }
            public IPlayer BlackPlayer { get; set; }
            public String Event { get; set; }
            public String Site { get; set; }
            public Outcome Result { get; set; }
            public String ECO { get; set; }
            public int Round { get; set; }
            public String Date { get; set; }
            public GameDetails()
            {
                WhitePlayer = new User("");
                BlackPlayer = new User("");
                Event = "?";
                Site = "?";
                Result = Outcome.NotAvailable;
                ECO = "?";
                Round = 0;
                Date = "?";
                RegularDateString = "";
                isUserGame = true;
            }
        }
        public class PanelProfile
        {
            public PanelProfile(ChessFont chessFont)
            {
                if (chessFont == ChessFont.Merida)
                {
                    PawnSize = Tuple.Create(1 / 10F, 2 / 3F);
                    KnightSize = Tuple.Create(1 / 9F, 9 / 10F);
                    BishopSize = Tuple.Create(1 / 9F, 1F);
                    RookSize = Tuple.Create(1 / 10F, 9 / 10F);
                    QueenSize = Tuple.Create(1 / 9F, 9 / 10F);
                    KingSize = Tuple.Create(1 / 9F, 1F);
                }
                if (chessFont == ChessFont.Berlin)
                {
                    PawnSize = Tuple.Create(2 / 17F, 9 / 10F);
                    KnightSize = Tuple.Create(1 / 8F, 9 / 10F);
                    BishopSize = Tuple.Create(1 / 8F, 1F);
                    RookSize = Tuple.Create(1 / 8F, 9 / 10F);
                    QueenSize = Tuple.Create(1 / 8F, 9 / 10F);
                    KingSize = Tuple.Create(1 / 8F, 1F);
                }
            }
            /// <summary>
            /// ImageHeight is panel1.Height * item1; Image width is ImageHeight * item2
            /// </summary>
            public Tuple<float, float> BishopSize { get; set; }
            /// <summary>
            /// ImageHeight is panel1.Height * item1; Image width is ImageHeight * item2
            /// </summary>
            public Tuple<float, float> KnightSize { get; set; }
            /// <summary>
            /// ImageHeight is panel1.Height * item1; Image width is ImageHeight * item2
            /// </summary>
            public Tuple<float, float> QueenSize { get; set; }
            /// <summary>
            /// ImageHeight is panel1.Height * item1; Image width is ImageHeight * item2
            /// </summary>
            public Tuple<float, float> RookSize { get; set; }
            /// <summary>
            /// ImageHeight is panel1.Height * item1; Image width is ImageHeight * item2
            /// </summary>
            public Tuple<float, float> KingSize { get; set; }
            /// <summary>
            /// ImageHeight is panel1.Height * item1; Image width is ImageHeight * item2
            /// </summary>
            public Tuple<float, float> PawnSize { get; set; }
            public bool isActive { get; set; }

            public String ChessFontName { get; set; }
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettingsForm();
        }
    }
}