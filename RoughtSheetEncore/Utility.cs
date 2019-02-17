using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RoughtSheetEncore
{
    partial class Form1 : Form
    {
        enum RedrawPerspective
        {
            Arrow,
            LastMoveHL,
            CheckedKingHL,
            LegalMoveHL,
            UserArrows,
            None,
        }

        #region Declarations
        Clock BlackTime;
        Clock WhiteTime;
        bool ShouldClockTick = true, isBoardFlipped, ShouldHLLegalSquares;
        System.Windows.Forms.Timer ClockTimer;
        static Graphics graphics;
        List<AnimationTask> AnimationList;
        List<Square> InvalidatedSquares;
        System.Windows.Forms.Timer AnimationTimer;
        Square CheckedKingHLSquare;
        IPlayer CurrentPlayer;
        Form EDForm;
        Panel EDPanel = new Panel(), EDCordPanel;
        Bitmap EDImage;
        System.Windows.Forms.Timer EDRefreshTimer;
        Piece EDPieceSelected;
        CheckBox EDQSB = new CheckBox(), EDKSB = new CheckBox(), 
            EDQSW = new CheckBox(), EDKSW = new CheckBox(), EDInvertPieces = new CheckBox();
        RadioButton EDWTPlay = new RadioButton(), EDBTPlay = new RadioButton();
        TextBox EDEnPassantSQ = new TextBox(), EDStartNo = new TextBox(), EDFEN = new TextBox();
        bool EDTest, ShouldUseEtiquette;
#endregion

        private void SetUpClocks_Utilities()
        {
            if (graphics == null)
            {
                graphics = panel1.CreateGraphics();
                graphics.InterpolationMode = InterpolationMode.Low;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            }

            RunOtherUtilities();

            ClockTimer = new System.Windows.Forms.Timer();
            ClockTimer.Interval = 500;
            ClockTimer.Enabled = true;
            ClockTimer.Tick += ClockTimer_Tick;
            WhiteClockLabel.Text = WhiteTime.ToString(true);
            BlackClockLabel.Text = BlackTime.ToString(true);
            WhiteClockLabel.Tag = WhiteTime.Player;
            BlackClockLabel.Tag = BlackTime.Player;

            AnimationList = new List<AnimationTask>();
            AnimationTimer = new System.Windows.Forms.Timer();
            AnimationTimer.Enabled = true;
            ShouldAnimate = settings.appearance.AnimationType != "None";
            AnimationTimer.Stop();
            AnimationTimer.Tick += AnimationTimer_Tick;
            if (settings.appearance.AnimationType == "Fast")
                AnimationTimer.Interval = 30;
            else if (settings.appearance.AnimationType == "Slow")
                AnimationTimer.Interval = 50;
        }
        private void ParseFENString(String p)
        {
            Pieces = new List<Piece>();
            CapturedPieces = new List<Piece>();
            GameVariations = new List<VariationHolder>();
            CurrentVariation = new VariationHolder();
            MainLine = CurrentVariation;
            CurrentVariation.MovesList = new List<Position>();
            GameVariations.Add(CurrentVariation);
            CurrentPosition = new Position();
            CurrentVariation.MovesList.Add(CurrentPosition);

            Square CurrentSquare = null;
            String div = "";
            foreach (var item in Squares)
                item.Piece = null;
            for (int i = 7; i >= 0; i--)
            {
                int file = 0;
                CurrentSquare = TwoDSquares[i, file];
                if (p.IndexOf('/') < 0)
                {
                    if (p.IndexOf(' ') > 0)
                    {
                        div = p.Substring(0, p.IndexOf(' '));
                        p = p.Substring(p.IndexOf(' ') + 1);
                    }
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    div = p.Substring(0, p.IndexOf('/'));
                    p = p.Substring(p.IndexOf('/') + 1);
                }

                // Tests. . .
                if (div.Length > 8)
                    throw new InvalidOperationException();
                int test = 0;
                String buffer = "";
                foreach (var item in div)
                {
                    if (Char.IsNumber(item))
                        test += int.Parse(item.ToString());
                    else if (Char.IsLetter(item))
                        buffer += item;
                    else
                        throw new InvalidOperationException();
                }
                if (buffer.Length + test < 8)
                    throw new InvalidOperationException();

                foreach (var item in div)
                {
                    if (char.IsNumber(item))
                    {
                        int x = int.Parse(item.ToString());
                        if (x > 8)
                            throw new InvalidOperationException();
                        if (x + file < 8)
                        {
                            file += x;
                            CurrentSquare = TwoDSquares[i, file];
                        }
                        else
                            file = 0;
                    }
                    else
                    {
                        Piece tempPiece = null;
                        switch (item)
                        {
                            case 'p': tempPiece = new Piece(Piece.PieceSide.Black, Piece.PieceType.Pawn);
                                break;
                            case 'P': tempPiece = new Piece(Piece.PieceSide.White, Piece.PieceType.Pawn);
                                break;
                            case 'n': tempPiece = new Piece(Piece.PieceSide.Black, Piece.PieceType.Knight);
                                break;
                            case 'N': tempPiece = new Piece(Piece.PieceSide.White, Piece.PieceType.Knight);
                                break;
                            case 'b': tempPiece = new Piece(Piece.PieceSide.Black, Piece.PieceType.Bishop);
                                break;
                            case 'B': tempPiece = new Piece(Piece.PieceSide.White, Piece.PieceType.Bishop);
                                break;
                            case 'r': tempPiece = new Piece(Piece.PieceSide.Black, Piece.PieceType.Rook);
                                break;
                            case 'R': tempPiece = new Piece(Piece.PieceSide.White, Piece.PieceType.Rook);
                                break;
                            case 'q': tempPiece = new Piece(Piece.PieceSide.Black, Piece.PieceType.Queen);
                                break;
                            case 'Q': tempPiece = new Piece(Piece.PieceSide.White, Piece.PieceType.Queen);
                                break;
                            case 'k': tempPiece = new Piece(Piece.PieceSide.Black, Piece.PieceType.King);
                                break;
                            case 'K': tempPiece = new Piece(Piece.PieceSide.White, Piece.PieceType.King);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        Pieces.Add(tempPiece);
                        CurrentSquare.Piece = tempPiece;
                        tempPiece.Square = CurrentSquare;

                        file++;
                        if (file < 8)
                            CurrentSquare = TwoDSquares[i, file];
                        else
                            file = 0;
                    }
                } 
            }
            
            CurrentPosition.PieceInfos = ClonePieces();

            // sideToPlay follows
            if (p.IndexOf(' ') > 0)
            {
                div = p.Substring(0, p.IndexOf(' '));
                p = p.Substring(p.IndexOf(' ') + 1);
                if (String.Compare(div, "w", true) == 0)
                    CurrentPosition.sideToPlay = Piece.PieceSide.White;
                else if (String.Compare(div, "b", true) == 0)
                    CurrentPosition.sideToPlay = Piece.PieceSide.Black;
                else
                    throw new InvalidOperationException();
            }
            else if (p.IndexOfAny(new Char[] { 'w', 'W', 'b', 'B' }) >= 0)
            {
                div = p;
                p = "";
                div = p.Substring(0, p.IndexOf(' '));
                p = p.Substring(p.IndexOf(' ') + 1);
                if (String.Compare(div, "w", true) == 0)
                    CurrentPosition.sideToPlay = Piece.PieceSide.White;
                else if (String.Compare(div, "b", true) == 0)
                    CurrentPosition.sideToPlay = Piece.PieceSide.Black;
                else
                    throw new InvalidOperationException();
            }
            else
                throw new InvalidOperationException();

            //Castling options follow
            if (p.IndexOf(' ') > 0)
            {
                div = p.Substring(0, p.IndexOf(' '));
                p = p.Substring(p.IndexOf(' ') + 1);

                CurrentPosition.KingsideCastlingWhite = div.Contains("K");
                CurrentPosition.QueensideCastlingWhite = div.Contains("Q");
                CurrentPosition.KingsideCastlingBlack = div.Contains("k");
                CurrentPosition.QueensideCastlingBlack = div.Contains("q");
            }
            else if (p.IndexOfAny(new char[] { 'Q', 'q', 'K', 'k' }) >= 0)
            {
                div = p;
                p = "";
                CurrentPosition.KingsideCastlingWhite = div.Contains("K");
                CurrentPosition.QueensideCastlingWhite = div.Contains("Q");
                CurrentPosition.KingsideCastlingBlack = div.Contains("k");
                CurrentPosition.QueensideCastlingBlack = div.Contains("q");
            }

            //enPassant follows
            if (p.IndexOf(' ') > 0)
            {
                div = p.Substring(0, p.IndexOf(' '));
                p = p.Substring(p.IndexOf(' ') + 1);
                Square square = null;
                if (TryGetSquare(div, out square))
                {
                    if (int.Parse(square.Name.Substring(1, 1)) == 3)
                        square = GetSquare(square.Name.Substring(0, 1) + 4);
                    else if (int.Parse(square.Name.Substring(1, 1)) == 6)
                        square = GetSquare(square.Name.Substring(0, 1) + 5);
                    else
                        square = null;
                    if (square != null && square.Piece != null && square.Piece.Type == Piece.PieceType.Pawn)
                        CurrentPosition.EnPassantPawn = square.Piece;
                }
            }
            else if (p.Length >= 2)
            {
                div = p;
                p = "";
                Square square = null;
                if (TryGetSquare(div, out square))
                {
                    if (int.Parse(square.Name.Substring(1, 1)) == 3)
                        square = GetSquare(square.Name.Substring(0, 1) + 4);
                    else if (int.Parse(square.Name.Substring(1, 1)) == 6)
                        square = GetSquare(square.Name.Substring(0, 1) + 5);
                    else
                        square = null;
                    if (square != null && square.Piece != null && square.Piece.Type == Piece.PieceType.Pawn)
                        CurrentPosition.EnPassantPawn = square.Piece;
                }
            }

            Graphics g = panel1.CreateGraphics();
            foreach (var item in Squares)
            {
                g.FillRectangle((item.Type == Square.SquareType.Light ? LightSquareColor : DarkSquareColor), item.Rectangle);
                if (item.Piece != null)
                    PlacePiece(item.Piece, item);
            }
            EnPassantPawn = CurrentPosition.EnPassantPawn;
            KingsideCastlingBlack = CurrentPosition.KingsideCastlingBlack;
            KingsideCastlingWhite = CurrentPosition.KingsideCastlingWhite;
            QueensideCastlingBlack = CurrentPosition.QueensideCastlingBlack;
            QueensideCastlingWhite = CurrentPosition.QueensideCastlingWhite;
            sideToPlay = CurrentPosition.sideToPlay;
        }
        private bool LoadFEN(String p)
        {
            //Do tests after parsing FEN string to make sure position is valid

            List<Piece> _Pieces = Pieces;
            List<Piece> _CapturedPieces = CapturedPieces;
            List<VariationHolder> _GameVariations = GameVariations;
            VariationHolder _CurrentVariation = CurrentVariation;
            Position _CurrentPosition = CurrentPosition;
            try
            {
                ParseFENString(p);
            }
            catch (Exception)
            {
                Pieces = _Pieces;
                CapturedPieces = _CapturedPieces;
                GameVariations = _GameVariations;
                CurrentVariation = _CurrentVariation;
                CurrentPosition = _CurrentPosition;
                MainLine = CurrentVariation;
                MessageBox.Show("Loading Error! Wrong FEN format");
                return false;
            }

            foreach (var item in Squares)
            {
                if (item.Piece != null)
                {
                    Pieces.Add(item.Piece);
                    if (item.Piece.Name == "White King")
                        WhiteKing = item.Piece;
                    else if (item.Piece.Name == "Black King")
                        BlackKing = item.Piece;
                }
            }

            FENforStartPos = p;
            foreach (var item in LoadedEngines)
                SendPositionToEngine(item, "");
            OpeningLabel.Text = String.Format("From POSITION");
            toolTip1.SetToolTip(OpeningLabel, p);
            OpeningLabel.Location = new Point(tabPage1.Width / 2 - OpeningLabel.Width / 2, flowLayoutPanel1.Top - 30);
            SSOpening.Text = String.Format("From POSITION");
            toolTip1.SetToolTip(SSOpening, p);
            SSOpening.Location = new Point(tabPage2.Width / 2 - SSOpening.Width / 2, SSOpening.Location.Y);

            flowLayoutPanel1.Controls.Clear();
            SSPanel.Controls.Clear();
            return true;
        }
        private String GetFENString()
        {
            String buffer = "";
            for (int i = 7; i >= 0; i--)
            {
                int count = 0;
                for (int j = 0; j <= 7; j++)
                {
                    if (TwoDSquares[i, j].Piece == null)
                        count++;
                    else
                    {
                        if (count > 0)
                        {
                            buffer += count.ToString();
                            count = 0;
                        }
                        switch (TwoDSquares[i, j].Piece.Type)
                        {
                            case Piece.PieceType.King:
                                buffer += (TwoDSquares[i, j].Piece.Side == Piece.PieceSide.White ? "K" : "k");
                                break;
                            case Piece.PieceType.Queen:
                                buffer += (TwoDSquares[i, j].Piece.Side == Piece.PieceSide.White ? "Q" : "q");
                                break;
                            case Piece.PieceType.Rook:
                                buffer += (TwoDSquares[i, j].Piece.Side == Piece.PieceSide.White ? "R" : "r");
                                break;
                            case Piece.PieceType.Bishop:
                                buffer += (TwoDSquares[i, j].Piece.Side == Piece.PieceSide.White ? "B" : "b");
                                break;
                            case Piece.PieceType.Knight:
                                buffer += (TwoDSquares[i, j].Piece.Side == Piece.PieceSide.White ? "N" : "n");
                                break;
                            case Piece.PieceType.Pawn:
                                buffer += (TwoDSquares[i, j].Piece.Side == Piece.PieceSide.White ? "P" : "p");
                                break;
                        }
                    }
                }
                if (count > 0)
                    buffer += count.ToString();
                if (i != 0)
                    buffer += "/";
            }
            buffer += " " + (sideToPlay == Piece.PieceSide.White ? "w" : "b");
            if (!KingsideCastlingBlack && !QueensideCastlingBlack
                && !KingsideCastlingWhite && !QueensideCastlingWhite)
                buffer += " " + "-";
            else
                buffer += " " + (KingsideCastlingWhite ? "K" : "") + (QueensideCastlingWhite ? "Q" : "")
                + (KingsideCastlingBlack ? "k" : "") + (QueensideCastlingBlack ? "q" : "") + " ";

            if (CurrentPosition.LastMovePlayed != null)
            {
                Move move = CurrentPosition.LastMovePlayed;
                if (move.PieceMoving.Type == Piece.PieceType.Pawn)
                {
                    int x = int.Parse(move.OriginalSquare.Name.Substring(1, 1)) - int.Parse(move.DestSquare.Name.Substring(1, 1));
                    if (x == -2)
                    {
                        buffer += move.OriginalSquare.Name.Substring(0, 1) +
                            (int.Parse(move.OriginalSquare.Name.Substring(1, 1)) + 1).ToString();
                    }
                    else if (x == 2)
                    {
                        buffer += move.OriginalSquare.Name.Substring(0, 1) +
                            (int.Parse(move.OriginalSquare.Name.Substring(1, 1)) - 1).ToString();
                    }
                    else
                        buffer += "-";
                }
                else
                    buffer += "-";
            }
            else
                buffer += "-";

            buffer += " " + FiftyMoveCount.ToString();
            buffer += " " + ((MoveCount + 2) / 2).ToString();

            return buffer;
        }
        private void editPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEDForm();
        }
        private void ShowEDForm()
        {
            #region Form Creation
            if (EDForm == null)
            {
                EDForm = new Form();
                EDForm.Text = "Edit Position";
                EDForm.KeyDown += EDForm_KeyDown;
                EDRefreshTimer = new System.Windows.Forms.Timer();
                EDRefreshTimer.Interval = 30;
                EDRefreshTimer.Tick += EDRefreshTimer_Tick;
                EDPanel.Location = new Point(10, 10);
                EDForm.Size = new System.Drawing.Size(800, 550);
                EDForm.MaximizeBox = false;
                EDForm.MinimizeBox = false;
                EDForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                
                PictureBox KingWPB = new PictureBox();
                KingWPB.Image = WhiteKingImage;
                KingWPB.SizeMode = PictureBoxSizeMode.Zoom;
                KingWPB.Size = new System.Drawing.Size(35, 35);
                KingWPB.Location = new Point(450, 50);
                KingWPB.Tag = new Piece(Piece.PieceSide.White, Piece.PieceType.King);
                KingWPB.BorderStyle = BorderStyle.FixedSingle;
                KingWPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(KingWPB);
                PictureBox QueenWPB = new PictureBox();
                QueenWPB.Image = WhiteQueenImage;
                QueenWPB.SizeMode = PictureBoxSizeMode.Zoom;
                QueenWPB.Size = new System.Drawing.Size(35, 35);
                QueenWPB.Location = new Point(450, 100);
                QueenWPB.Tag = new Piece(Piece.PieceSide.White, Piece.PieceType.Queen);
                QueenWPB.BorderStyle = BorderStyle.FixedSingle;
                QueenWPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(QueenWPB);
                PictureBox RookWPB = new PictureBox();
                RookWPB.Image = WhiteRookImage;
                RookWPB.SizeMode = PictureBoxSizeMode.Zoom;
                RookWPB.Size = new System.Drawing.Size(35, 35);
                RookWPB.Location = new Point(450, 150);
                RookWPB.Tag = new Piece(Piece.PieceSide.White, Piece.PieceType.Rook);
                RookWPB.BorderStyle = BorderStyle.FixedSingle;
                RookWPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(RookWPB);
                PictureBox BishopWPB = new PictureBox();
                BishopWPB.Image = WhiteBishopImage;
                BishopWPB.SizeMode = PictureBoxSizeMode.Zoom;
                BishopWPB.Size = new System.Drawing.Size(35, 35);
                BishopWPB.Location = new Point(450, 200);
                BishopWPB.Tag = new Piece(Piece.PieceSide.White, Piece.PieceType.Bishop);
                BishopWPB.BorderStyle = BorderStyle.FixedSingle;
                BishopWPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(BishopWPB);
                PictureBox KnightWPB = new PictureBox();
                KnightWPB.Image = WhiteKnightImage;
                KnightWPB.SizeMode = PictureBoxSizeMode.Zoom;
                KnightWPB.Size = new System.Drawing.Size(35, 35);
                KnightWPB.Location = new Point(450, 250);
                KnightWPB.Tag = new Piece(Piece.PieceSide.White, Piece.PieceType.Knight);
                KnightWPB.BorderStyle = BorderStyle.FixedSingle;
                KnightWPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(KnightWPB);
                PictureBox PawnWPB = new PictureBox();
                PawnWPB.Image = WhitePawnImage;
                PawnWPB.SizeMode = PictureBoxSizeMode.Zoom;
                PawnWPB.Size = new System.Drawing.Size(35, 35);
                PawnWPB.Location = new Point(450, 300);
                PawnWPB.Tag = new Piece(Piece.PieceSide.White, Piece.PieceType.Pawn);
                PawnWPB.BorderStyle = BorderStyle.FixedSingle;
                PawnWPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(PawnWPB);

                PictureBox KingBPB = new PictureBox();
                KingBPB.Image = BlackKingImage;
                KingBPB.SizeMode = PictureBoxSizeMode.Zoom;
                KingBPB.Size = new System.Drawing.Size(35, 35);
                KingBPB.Location = new Point(500, 50);
                KingBPB.Tag = new Piece(Piece.PieceSide.Black, Piece.PieceType.King);
                KingBPB.BorderStyle = BorderStyle.FixedSingle;
                KingBPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(KingBPB);
                PictureBox QueenBPB = new PictureBox();
                QueenBPB.Image = BlackQueenImage;
                QueenBPB.SizeMode = PictureBoxSizeMode.Zoom;
                QueenBPB.Size = new System.Drawing.Size(35, 35);
                QueenBPB.Location = new Point(500, 100);
                QueenBPB.Tag = new Piece(Piece.PieceSide.Black, Piece.PieceType.Queen);
                QueenBPB.BorderStyle = BorderStyle.FixedSingle;
                QueenBPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(QueenBPB);
                PictureBox RookBPB = new PictureBox();
                RookBPB.Image = BlackRookImage;
                RookBPB.SizeMode = PictureBoxSizeMode.Zoom;
                RookBPB.Size = new System.Drawing.Size(35, 35);
                RookBPB.Location = new Point(500, 150);
                RookBPB.Tag = new Piece(Piece.PieceSide.Black, Piece.PieceType.Rook);
                RookBPB.BorderStyle = BorderStyle.FixedSingle;
                RookBPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(RookBPB);
                PictureBox BishopBPB = new PictureBox();
                BishopBPB.Image = BlackBishopImage;
                BishopBPB.SizeMode = PictureBoxSizeMode.Zoom;
                BishopBPB.Size = new System.Drawing.Size(35, 35);
                BishopBPB.Location = new Point(500, 200);
                BishopBPB.Tag = new Piece(Piece.PieceSide.Black, Piece.PieceType.Bishop);
                BishopBPB.BorderStyle = BorderStyle.FixedSingle;
                BishopBPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(BishopBPB);
                PictureBox KnightBPB = new PictureBox();
                KnightBPB.Image = BlackKnightImage;
                KnightBPB.SizeMode = PictureBoxSizeMode.Zoom;
                KnightBPB.Size = new System.Drawing.Size(35, 35);
                KnightBPB.Location = new Point(500, 250);
                KnightBPB.Tag = new Piece(Piece.PieceSide.Black, Piece.PieceType.Knight);
                KnightBPB.BorderStyle = BorderStyle.FixedSingle;
                KnightBPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(KnightBPB);
                PictureBox PawnBPB = new PictureBox();
                PawnBPB.Image = BlackPawnImage;
                PawnBPB.SizeMode = PictureBoxSizeMode.Zoom;
                PawnBPB.Size = new System.Drawing.Size(35, 35);
                PawnBPB.Location = new Point(500, 300);
                PawnBPB.Tag = new Piece(Piece.PieceSide.Black, Piece.PieceType.Pawn);
                PawnBPB.BorderStyle = BorderStyle.FixedSingle;
                PawnBPB.Click += EDPieceSelect_Click;
                EDForm.Controls.Add(PawnBPB);

                EDImage = new Bitmap(EDPanel.Width, EDPanel.Height);
                EDPanel.Paint += EDPanel_Paint;
                EDPanel.MouseMove += EDPanel_MouseMove;
                EDPanel.MouseDown += EDPanel_MouseDown;
                EDPanel.MouseLeave += EDPanel_MouseLeave;
                EDPanel.MouseEnter += EDPanel_MouseEnter;

                EDCordPanel = new Panel();
                EDForm.Controls.Add(EDCordPanel);
                EDCordPanel.Controls.Add(EDPanel);
                EDCordPanel.BackColor = Color.Gray;
                EDCordPanel.Size = EDPanel.Size + new Size(30, 30);
                EDCordPanel.Location = new Point(0, 0);
                EDPanel.Location = new Point(15, 15);

                GroupBox STPlay = new GroupBox(), CastlingOpt = new GroupBox();
                EDForm.Controls.Add(STPlay);
                EDForm.Controls.Add(CastlingOpt);
                STPlay.Text = "SIDE TO PLAY";
                CastlingOpt.Text = "CASTLING OPTIONS";
                CastlingOpt.Location = new Point(550, 20);
                CastlingOpt.Controls.Add(EDQSB); CastlingOpt.Controls.Add(EDKSB);
                CastlingOpt.Controls.Add(EDQSW); CastlingOpt.Controls.Add(EDKSW);
                EDKSW.Text = "White O-O";
                EDQSW.Text = "White O-O-O";
                EDKSB.Text = "Black O-O";
                EDQSB.Text = "Black O-O-O";
                EDQSB.CheckedChanged += CastlingOpt_CheckedChanged;
                EDKSB.CheckedChanged += CastlingOpt_CheckedChanged;
                EDQSW.CheckedChanged += CastlingOpt_CheckedChanged;
                EDKSW.CheckedChanged += CastlingOpt_CheckedChanged;
                Font font = new System.Drawing.Font("Caibri", 8.5F, FontStyle.Regular);
                CastlingOpt.Font = font;
                STPlay.Font = font;
                EDKSW.Font = font; EDKSB.Font = font;
                EDQSW.Font = font; EDQSB.Font = font;
                EDKSW.Location = new Point(5, 40);
                EDQSW.Location = new Point(5, 60);
                EDKSB.Location = new Point(5, 80);
                EDQSB.Location = new Point(5, 100);
                CastlingOpt.Size = new System.Drawing.Size(100, 120);
                STPlay.Location = new Point(550, CastlingOpt.Bottom + 20);
                STPlay.Size = new System.Drawing.Size(100, 60);
                EDWTPlay.Text = "White";
                EDBTPlay.Text = "Black";
                EDWTPlay.Font = font; EDBTPlay.Font = font;
                STPlay.Controls.Add(EDWTPlay);
                STPlay.Controls.Add(EDBTPlay);
                EDWTPlay.CheckedChanged += EDSTPlay_CheckedChanged;
                EDBTPlay.CheckedChanged += EDSTPlay_CheckedChanged;
                EDWTPlay.Location = new Point(5, 20);
                EDBTPlay.Location = new Point(5, 40);

                Label EnPassantLabel = new Label();
                EDForm.Controls.Add(EDEnPassantSQ);
                EDForm.Controls.Add(EnPassantLabel);
                EnPassantLabel.Location = new Point(550, STPlay.Bottom + 20);
                EnPassantLabel.Font = font;
                EnPassantLabel.AutoSize = true;
                EnPassantLabel.Text = "En Passant square";
                EDEnPassantSQ.Location = new Point(EnPassantLabel.Right + 38, EnPassantLabel.Location.Y);
                EDEnPassantSQ.MaxLength = 2;
                EDEnPassantSQ.Width = 25;

                Label StartNoLabel = new Label();
                EDForm.Controls.Add(EDStartNo);
                EDForm.Controls.Add(StartNoLabel);
                StartNoLabel.Location = new Point(550, EnPassantLabel.Bottom + 15);
                StartNoLabel.Font = font;
                StartNoLabel.AutoSize = true;
                StartNoLabel.Text = "Starting Move number";
                EDStartNo.Location = new Point(StartNoLabel.Right + 10, StartNoLabel.Location.Y);
                EDStartNo.Width = 35;

                EDForm.Controls.Add(EDInvertPieces);
                EDInvertPieces.Location = new Point(550, StartNoLabel.Bottom + 20);
                EDInvertPieces.Font = font;
                EDInvertPieces.AutoSize = true;
                EDInvertPieces.Text = "Invert Pieces (leaving board constant)";
                EDInvertPieces.CheckedChanged += InvertPieces_CheckedChanged;

                Label EDFENLabel = new Label();
                EDForm.Controls.Add(EDFENLabel);
                EDForm.Controls.Add(EDFEN);
                EDFENLabel.Location = new Point(450, EDInvertPieces.Bottom + 30);
                EDFENLabel.Font = font;
                EDFEN.Font = font;
                EDFENLabel.AutoSize = true;
                EDFENLabel.Text = "Optionally specify FEN notation: ";
                EDFEN.Location = new Point(450, EDFENLabel.Bottom + 7);
                EDFEN.Multiline = true;
                EDFEN.Size = new System.Drawing.Size(300, 50);
                Padding = new Padding(4);

                Button EDCancel = new Button(), EDEnter = new Button();
                EDForm.Controls.Add(EDCancel);
                EDForm.Controls.Add(EDEnter);
                EDEnter.Text = "OK";
                EDCancel.Text = "Cancel";
                EDEnter.Location = new Point(700, 450);
                EDCancel.Location = new Point(600, 450);
                EDEnter.Size = new Size(60, 35);
                EDCancel.Size = new Size(70, 35);
                EDEnter.Font = new Font("Calibri", 13F, FontStyle.Bold);
                EDCancel.Font = new Font("Calibri", 13F, FontStyle.Bold);
                EDEnter.Click += EDEnter_Click;
                EDCancel.Click += EDCancel_Click;

                Button EDCurrPosition = new Button(), EDClearBoard = new Button(), EDStartPos = new Button();
                EDForm.Controls.Add(EDCurrPosition);
                EDForm.Controls.Add(EDClearBoard);
                EDForm.Controls.Add(EDStartPos);
                EDClearBoard.Text = "C&lear board";
                EDCurrPosition.Text = "&Current Position";
                EDStartPos.Text = "&Startposition";
                EDStartPos.AutoSize = true;
                EDCurrPosition.AutoSize = true;
                EDClearBoard.AutoSize = true;
                EDClearBoard.Location = new Point(10, 450);
                EDCurrPosition.Location = new Point(160, 450);
                EDStartPos.Location = new Point(330, 450);
                EDClearBoard.Font = new Font("Calibri", 11F, FontStyle.Bold);
                EDCurrPosition.Font = new Font("Calibri", 11F, FontStyle.Bold);
                EDStartPos.Font = new Font("Calibri", 11F, FontStyle.Bold);
                EDClearBoard.Click += EDClearBoard_Click;
                EDCurrPosition.Click += EDCurrPosition_Click;
                EDStartPos.Click += EDStartPos_Click;

                ToolTip tt = new ToolTip();
                tt.Active = true;
                tt.AutoPopDelay = 5000;
                tt.InitialDelay = 1000;
                tt.ReshowDelay = 500;
                tt.ShowAlways = true;
                tt.SetToolTip(this.EDFEN, "The FEN Notation here will override the other Edit options");
            }
            #endregion

            #region Intialization
            EDForm.KeyPreview = true;
            foreach (var square in Squares)
            {
                square.EDPiece = square.Piece;
            }

            EDForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            EDFEN.Text = "";
            EDInvertPieces.Checked = false;
            EDStartNo.Text = "1";
            EDEnPassantSQ.Text = "";
            if (CurrentPosition.EnPassantPawn != null)
            {
                Square sq = CurrentPosition.EnPassantPawn.Square;
                if (sq.Name.Substring(1, 1) == "4")
                    EDEnPassantSQ.Text = sq.Name.Substring(0, 1) + 3;
                else if (sq.Name.Substring(1, 1) == "5")
                    EDEnPassantSQ.Text = sq.Name.Substring(0, 1) + 6;
            }
            EDQSB.Checked = CurrentPosition.QueensideCastlingBlack;
            EDQSW.Checked = CurrentPosition.QueensideCastlingWhite;
            EDKSB.Checked = CurrentPosition.KingsideCastlingBlack;
            EDKSW.Checked = CurrentPosition.KingsideCastlingWhite;
            if (sideToPlay == Piece.PieceSide.White)
                EDWTPlay.Checked = true;
            else
                EDBTPlay.Checked = true;
            Graphics g = Graphics.FromImage(EDImage);

            foreach (var square in Squares)
            {
                g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor : LightSquareColor, square.EDRectangle);
                if (square.EDPiece != null)
                {
                    int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                    int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                    int dX = midX - square.EDPiece.Image.Width / 2;
                    int dY = midY - square.EDPiece.Image.Height / 2;
                    square.EDPiece.Location = new Point(dX, dY);
                    g.DrawImageUnscaled(square.EDPiece.Image, dX, dY);
                }
            }

            EDPieceSelected = new Piece(Piece.PieceSide.White, Piece.PieceType.King);

            EDRefreshTimer.Enabled = true;
            EDRefreshTimer.Start();
            #endregion

            EDForm.ShowDialog(this);

            #region Finalization
            EDRefreshTimer.Enabled = false;
            EDRefreshTimer.Stop();
            if (EDForm.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                GameDetails gd = new GameDetails();
                LoadEDPosition();

                HandleChecking(false);
                HandleDraws(null);

                Position TempPosition = new Position();
                GameVariations = new List<VariationHolder>();
                CurrentVariation = new VariationHolder(new List<Position>());
                MainLine = CurrentVariation;
                GameVariations.Add(CurrentVariation);
                TempPosition.CheckingPiece = CheckingPiece;
                TempPosition.DoubleCheckingPiece = DoubleCheckingPiece;
                TempPosition.IsBlackCheckmated = IsBlackCheckmated;
                TempPosition.IsBlackInCheck = IsBlackInCheck;
                TempPosition.IsDraw = IsDraw;
                TempPosition.IsWhiteCheckmated = IsWhiteCheckmated;
                TempPosition.IsWhiteInCheck = IsWhiteInCheck;
                TempPosition.KingsideCastlingBlack = KingsideCastlingBlack;
                TempPosition.KingsideCastlingWhite = KingsideCastlingWhite;
                TempPosition.EnPassantPawn = EnPassantPawn;
                TempPosition.PossibleDefences = PossibleDefences;
                TempPosition.PieceInfos = ClonePieces();
                TempPosition.QueensideCastlingBlack = QueensideCastlingBlack;
                TempPosition.QueensideCastlingWhite = QueensideCastlingWhite;
                TempPosition.sideToPlay = sideToPlay;
                TempPosition.MoveCount = MoveCount;
                TempPosition.FiftyMoveCount = FiftyMoveCount;
                CurrentVariation.MovesList.Add(TempPosition);
                CurrentPosition = TempPosition;

                if (IsWhiteCheckmated || IsBlackCheckmated)
                    MessageBox.Show("Checkmate. " + (IsBlackCheckmated ?
                    "White wins" : "Black Wins"));

                FENforStartPos = GetFENString();

                if (ModeOfPlay == PlayMode.EditPosition || true)
                {
                    SetGameDetails(FirstEngine, SecondEngine, gd);
                    ShowGameDetails();
                    return;
                    //SetGameDetails(CurrentUser, null, gd);
                }
                else if (ModeOfPlay == PlayMode.SinglePlayer)
                {
                    SetGameDetails(CurrentUser, FirstEngine, gd);
                }
                else
                {
                    ModeOfPlay = PlayMode.SinglePlayer;
                    SetGameDetails(CurrentUser, FirstEngine, gd);
                }
                ShowGameDetails();

                foreach (var item in LoadedEngines)
                {
                    if (item.isUciEngine)
                    {
                        SendPositionToEngine(item, "");
                        ClearEngineOutput(item);
                        if (isInfiniteSearch)
                        {
                            item.Process.StandardInput.WriteLine("go infinite");
                            BestMoveArrow.Enabled = true;
                        }
                    }
                }
            }
            else
            {
                CheckingPiece = CurrentPosition.CheckingPiece;
                EnPassantPawn = CurrentPosition.EnPassantPawn;
                DoubleCheckingPiece = CurrentPosition.DoubleCheckingPiece;
                PieceClicked = null;
                PromotePiece = null;
                panel1.Refresh();
            }
#endregion
        }
        void EDSTPlay_CheckedChanged(object sender, EventArgs e)
        {
            if (EDBTPlay.Checked)
                sideToPlay = Piece.PieceSide.Black;
            else if (EDWTPlay.Checked)
                sideToPlay = Piece.PieceSide.White;
        }
        void CastlingOpt_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (!cb.Checked)
                return;
            if (cb == EDQSW)
                EDQSW.Checked = QueensideCastlingWhite;
            else if (cb == EDQSB)
                EDQSB.Checked = QueensideCastlingBlack;
            else if (cb == EDKSW)
                EDKSW.Checked = KingsideCastlingWhite;
            else if (cb == EDKSB)
                EDKSB.Checked = KingsideCastlingBlack;
        }
        void EDStartPos_Click(object sender, EventArgs e)
        {
            foreach (var item in Squares)
                item.EDPiece = null;
            InitializePieces("EDIT");
            EDEnPassantSQ.Text = "";
            EDWTPlay.Checked = true;
            EDBTPlay.Checked = false;
            EDKSB.Checked = true;
            EDKSW.Checked = true;
            EDQSB.Checked = true;
            EDQSW.Checked = true;
            EDPanel_MouseMove(null, null);
        }
        private void EDClearBoard_Click(object sender, EventArgs e)
        {
            foreach (var item in Squares)
            {
                item.EDPiece = null;
            }
            UpdateEDCastlingOpt();
            EDEnPassantSQ.Text = "";
            EDPanel_MouseMove(null, null);
        }
        private void EDCurrPosition_Click(object sender, EventArgs e)
        {
            foreach (var item in Squares)
            {
                item.EDPiece = item.Piece;
            }
            EDPanel_MouseMove(null, null);
            if (CurrentPosition.EnPassantPawn != null)
                EDEnPassantSQ.Text = CurrentPosition.EnPassantPawn.Square.Name.Substring(0, 1) +
                    (CurrentPosition.EnPassantPawn.Side == Piece.PieceSide.White ? "3" : "6");
            EDBTPlay.Checked = sideToPlay == Piece.PieceSide.Black;
            EDWTPlay.Checked = sideToPlay == Piece.PieceSide.White;
            EDKSB.Checked = CurrentPosition.KingsideCastlingBlack;
            EDQSB.Checked = CurrentPosition.QueensideCastlingBlack;
            EDKSW.Checked = CurrentPosition.KingsideCastlingWhite;
            EDQSW.Checked = CurrentPosition.QueensideCastlingWhite;
        }
        void EDCancel_Click(object sender, EventArgs e)
        {
            LoadPosition(CurrentPosition, false);
            foreach (var item in Pieces)
            {
                item.Square.Piece = item;
            }
            EDForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            EDForm.Close();
        }
        void EDEnter_Click(object sender, EventArgs e)
        {
            RunEDTest();
            if (!EDTest)
                return;
            EDForm.DialogResult = System.Windows.Forms.DialogResult.OK;
            EDForm.Close();
        }
        void InvertPieces_CheckedChanged(object sender, EventArgs e)
        {
            InvertPieces();
            EDPanel_MouseMove(this, null);
        }
        void EDForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                EDCancel_Click(null, null);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                EDEnter_Click(null, null);
            }
            else if (e.KeyCode == Keys.L)
            {
                EDClearBoard_Click(null, null);
            }
            else if (e.KeyCode == Keys.S)
            {
                EDStartPos_Click(null, null);
            }
            else if (e.KeyCode == Keys.C)
            {
                EDCurrPosition_Click(null, null);
            }
        }
        void EDPanel_MouseEnter(object sender, EventArgs e)
        {
            
        }
        void EDPanel_MouseLeave(object sender, EventArgs e)
        {
            if (EDPieceSelected == null)
                return;
            Rectangle tempRect = new Rectangle(EDPieceSelected.Location, EDPieceSelected.Image.Size);
            Graphics g = Graphics.FromImage(EDImage);
            foreach (Square square in Squares)
                if (square.EDRectangle.IntersectsWith(tempRect))
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor : LightSquareColor, square.EDRectangle);
                    if (square.EDPiece != null)
                    {
                        int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                        int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                        int dX = midX - square.EDPiece.Image.Width / 2;
                        int dY = midY - square.EDPiece.Image.Height / 2;
                        square.EDPiece.Location = new Point(dX, dY);
                        g.DrawImageUnscaled(square.EDPiece.Image, dX, dY);
                    }
                }
            Cursor = Cursors.Default;
        }
        void EDPieceSelect_Click(object sender, EventArgs e)
        {
            EDPieceSelected = (sender as PictureBox).Tag as Piece;
        }
        void EDPanel_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var item in Squares)
            {
                if (item.EDRectangle.Contains(e.Location))
                {
                    bool ShouldUpdateCastling = false;
                    Piece tempPiece = null;
                    if (EDPieceSelected != null)
                    {
                        if (item == GetSquare("a1") || item == GetSquare("h1") || item == GetSquare("a8")
                        || item == GetSquare("h8") || item == GetSquare("e1") || item == GetSquare("e8") ||
                        (item.EDPiece != null && item.EDPiece.Type == Piece.PieceType.King) ||
                        EDPieceSelected.Type == Piece.PieceType.King)
                            ShouldUpdateCastling = true;

                        if (item.EDPiece != null && EDPieceSelected.Side == 
                            item.EDPiece.Side && EDPieceSelected.Type == item.EDPiece.Type)
                            tempPiece = new Piece((EDPieceSelected.Side == Piece.PieceSide.White ?
                                Piece.PieceSide.Black : Piece.PieceSide.White), EDPieceSelected.Type);
                        else if (item.EDPiece != null && EDPieceSelected.Side != 
                            item.EDPiece.Side && EDPieceSelected.Type == item.EDPiece.Type)
                            item.EDPiece = null;
                        else
                            tempPiece = new Piece((e.Button == System.Windows.Forms.MouseButtons.Left ?
                                EDPieceSelected.Side : (EDPieceSelected.Side == Piece.PieceSide.White ?
                                Piece.PieceSide.Black : Piece.PieceSide.White)), EDPieceSelected.Type);

                        item.EDPiece = tempPiece;
                        if (tempPiece != null)
                            tempPiece.Square = item;
                        if (item.EDPiece != null)
                        if (item.EDPiece.Type == Piece.PieceType.King)
                        {
                            foreach (var square in Squares)
                            {
                                if (square.EDPiece != null)
                                    if (square.EDPiece.Type == Piece.PieceType.King && 
                                        square.EDPiece.Side == item.EDPiece.Side && square != item)
                                    {
                                        square.EDPiece.Square = null;
                                        square.EDPiece = null;
                                        EDPanel_MouseMove(null, null);
                                    }
                            }
                        }
                    }
                    if (ShouldUpdateCastling)
                        UpdateEDCastlingOpt();
                }
            }
        }
        private void UpdateEDCastlingOpt()
        {
            Piece WKing = null, BKing = null, QRB = null, QRW = null, KRB = null, KRW = null;
            foreach (var item in Squares)
            {
                if (item.EDPiece != null)
                {
                    if (item.EDPiece.Name == "White King" && item.Name == "e1")
                        WKing = item.EDPiece;
                    else if (item.EDPiece.Name == "Black King" && item.Name == "e8")
                        BKing = item.EDPiece;
                    else if (item.EDPiece.Name == "White Rook" && item.Name == "a1")
                        QRW = item.EDPiece;
                    else if (item.EDPiece.Name == "White Rook" && item.Name == "h1")
                        KRW = item.EDPiece;
                    else if (item.EDPiece.Name == "Black Rook" && item.Name == "a8")
                        QRB = item.EDPiece;
                    else if (item.EDPiece.Name == "Black Rook" && item.Name == "h8")
                        KRB = item.EDPiece;
                }
            }

            EDKSW.Checked = WKing != null && KRW != null;
            EDKSB.Checked = BKing != null && KRB != null;
            EDQSW.Checked = WKing != null && QRW != null;
            EDQSB.Checked = BKing != null && QRB != null;

            KingsideCastlingBlack = EDKSB.Checked;
            KingsideCastlingWhite = EDKSW.Checked;
            QueensideCastlingBlack = EDQSB.Checked;
            QueensideCastlingWhite = EDQSW.Checked;
        }
        private void RunEDTest()
        {
            //FEN String
            if (EDFEN.Text != "")
            {
                if (LoadFEN(EDFEN.Text))
                {
                    EDTest = false;
                    EDForm.DialogResult = System.Windows.Forms.DialogResult.Ignore;
                    EDForm.Close();
                    return;
                }
                else
                {
                    EDTest = false;
                    MessageBox.Show("Setup Error! The FEN notation provided is in wrong format"
                            + "\nEnter valid FEN notation or leave the field empty");
                    return;
                }
            }

            List<Piece> _Pieces = new List<Piece>();
            _Pieces.AddRange(Pieces);
            Pieces = new List<Piece>();
            foreach (var item in Squares)
            {
                Piece temp = item.Piece;
                item.Piece = item.EDPiece;
                item.EDPiece = temp;
                if (item.Piece != null)
                {
                    Pieces.Add(item.Piece);
                    item.Piece.Square = item;
                }
            }

            //Check EnPassant square
            if (EDEnPassantSQ.Text != "")
            {
                Square square;
                if (TryGetSquare(EDEnPassantSQ.Text, out square))
                {
                    Piece piece = GetSquare(square.Name.Substring(0, 1) + "4").Piece,
                        piece2 = GetSquare(square.Name.Substring(0, 1) + "5").Piece;
                    if (!((square.Name.Substring(1, 1) == "3" && piece != null && piece.Type == Piece.PieceType.Pawn
                        && piece.Side == Piece.PieceSide.White) ||
                         (square.Name.Substring(1, 1) == "6" && piece2 != null &&
                        piece2.Type == Piece.PieceType.Pawn && piece2.Side == Piece.PieceSide.Black)))
                    {
                        EDTest = false;
                        MessageBox.Show("Setup Error! The square in the field: \"En Passant square\""
                            + "is not a valid En Passant square.\nEnter a valid square cordinate or leave the field empty");
                        Pieces = _Pieces;
                        foreach (var item in Squares)
                        {
                            Piece temp = item.Piece;
                            item.Piece = item.EDPiece;
                            item.EDPiece = temp;
                            if (item.Piece != null)
                                item.Piece.Square = item;
                        }
                        return;
                    }
                }
                else
                {
                    EDTest = false;
                    MessageBox.Show("Setup Error! The data you entered in the field: \"En Passant square\""
                        + "is not a valid square cordinate.\nEnter a valid square cordinate or leave the field empty");
                    Pieces = _Pieces;
                    foreach (var item in Squares)
                    {
                        Piece temp = item.Piece;
                        item.Piece = item.EDPiece;
                        item.EDPiece = temp;
                        if (item.Piece != null)
                            item.Piece.Square = item;
                    }
                    return;
                }
            }

            //Starting Move number
            if (EDStartNo.Text != "1" || EDStartNo.Text != "")
            {
                int x;
                if (int.TryParse(EDStartNo.Text, out x))
                {
                    if (x <= 0 || x > 1000000)
                    {
                        MessageBox.Show("Setup Error! The Starting Move number must be a number between 1 and 1000000."
                            + "\nEnter a valid number or leave the field empty");
                        Pieces = _Pieces;
                        foreach (var item in Squares)
                        {
                            Piece temp = item.Piece;
                            item.Piece = item.EDPiece;
                            item.EDPiece = temp;
                            if (item.Piece != null)
                                item.Piece.Square = item;
                        }
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Setup Error! The Starting Move number must be a number between 1 and 1000000."
                            + "\nEnter a valid number or leave the field empty");
                    Pieces = _Pieces;
                    foreach (var item in Squares)
                    {
                        Piece temp = item.Piece;
                        item.Piece = item.EDPiece;
                        item.EDPiece = temp;
                        if (item.Piece != null)
                            item.Piece.Square = item;
                    }
                    return;
                }
            }

            //KingCount Section
            bool WKing = false, BKing = false;
            Piece WhiteK = null, BlackK = null;
            foreach (var item in Pieces)
            {
                if (item.Type == Piece.PieceType.King)
                {
                    if (item.Side == Piece.PieceSide.White)
                    {
                        if (!WKing)
                            WKing = true;
                        else
                        {
                            EDTest = false;
                            MessageBox.Show("Setup Error! Each side must have a single King");
                            Pieces = _Pieces;
                            foreach (var square in Squares)
                            {
                                Piece temp = square.Piece;
                                square.Piece = square.EDPiece;
                                square.EDPiece = temp;
                                if (square.Piece != null)
                                    square.Piece.Square = square;
                            }
                            return;
                        }
                        WhiteK = item;
                    }
                    else
                    {
                        if (!BKing)
                            BKing = true;
                        else
                        {
                            EDTest = false;
                            MessageBox.Show("Setup Error! Each side must have a single King");
                            Pieces = _Pieces;
                            foreach (var square in Squares)
                            {
                                Piece temp = square.Piece;
                                square.Piece = square.EDPiece;
                                square.EDPiece = temp;
                                if (square.Piece != null)
                                    square.Piece.Square = square;
                            }
                            return;
                        }
                        BlackK = item;
                    }
                }
            }
            if (!BKing || !WKing)
            {
                EDTest = false;
                MessageBox.Show("Setup Error! Each side must have a single King");
                Pieces = _Pieces;
                foreach (var square in Squares)
                {
                    Piece temp = square.Piece;
                    square.Piece = square.EDPiece;
                    square.EDPiece = temp;
                    if (square.Piece != null)
                        square.Piece.Square = square;
                }
                return;
            }

            //King Placement section
            tester = true;
            if (GetValidSquares(WhiteK).Contains(BlackK.Square))
            {
                EDTest = false;
                MessageBox.Show("Setup Error! Enemy Kings cannot occupy adjacent squares. \nCheck squares "
                + WhiteK.Square + " and " + BlackK.Square);
                Pieces = _Pieces;
                foreach (var square in Squares)
                {
                    Piece temp = square.Piece;
                    square.Piece = square.EDPiece;
                    square.EDPiece = temp;
                    if (square.Piece != null)
                        square.Piece.Square = square;
                }
                return;
            }
            tester = false;

            //"Illegal-rank" Pawn section
            for (int i = 0; i < 8; i++)
            {
                if (TwoDSquares[0, i].Piece != null && TwoDSquares[0, i].Piece.Type == Piece.PieceType.Pawn)
                {
                    EDTest = false;
                    MessageBox.Show("Setup Error! Pawns cannot occupy the first or eight ranks. \nCheck square " + TwoDSquares[0, i]);
                    Pieces = _Pieces;
                    foreach (var square in Squares)
                    {
                        Piece temp = square.Piece;
                        square.Piece = square.EDPiece;
                        square.EDPiece = temp;
                        if (square.Piece != null)
                            square.Piece.Square = square;
                    }
                    return;
                }
                if (TwoDSquares[7, i].Piece != null && TwoDSquares[7, i].Piece.Type == Piece.PieceType.Pawn)
                {
                    EDTest = false;
                    MessageBox.Show("Setup Error! Pawns cannot occupy the first or eight ranks. \nCheck square " + TwoDSquares[7, i]);
                    Pieces = _Pieces;
                    foreach (var square in Squares)
                    {
                        Piece temp = square.Piece;
                        square.Piece = square.EDPiece;
                        square.EDPiece = temp;
                        if (square.Piece != null)
                            square.Piece.Square = square;
                    }
                    return;
                }
            }

            //King safety section
            Parallel.ForEach(Pieces, CheckForCheck);
            sideToPlay = sideToPlay == Piece.PieceSide.White ? Piece.PieceSide.Black : Piece.PieceSide.White;
            Parallel.ForEach(Pieces, CheckForCheck);
            sideToPlay = sideToPlay == Piece.PieceSide.White ? Piece.PieceSide.Black : Piece.PieceSide.White;
            if (IsWhiteInCheck && sideToPlay == Piece.PieceSide.Black)
            {
                EDTest = false;
                MessageBox.Show("Setup Error! White is in check, so White must be the side to play.");
                Pieces = _Pieces;
                foreach (var square in Squares)
                {
                    Piece temp = square.Piece;
                    square.Piece = square.EDPiece;
                    square.EDPiece = temp;
                    if (square.Piece != null)
                        square.Piece.Square = square;
                }
                IsWhiteInCheck = false;
                IsBlackInCheck = false;
                return;
            }
            if (IsBlackInCheck && sideToPlay == Piece.PieceSide.White)
            {
                EDTest = false;
                MessageBox.Show("Setup Error! Black is in check, so Black must be the side to play.");
                Pieces = _Pieces;
                foreach (var square in Squares)
                {
                    Piece temp = square.Piece;
                    square.Piece = square.EDPiece;
                    square.EDPiece = temp;
                    if (square.Piece != null)
                        square.Piece.Square = square;
                }
                IsWhiteInCheck = false;
                IsBlackInCheck = false;
                return;
            }
            EDTest = true;
            Pieces = _Pieces;
            foreach (var square in Squares)
            {
                Piece temp = square.Piece;
                square.Piece = square.EDPiece;
                square.EDPiece = temp;
                if (square.Piece != null)
                    square.Piece.Square = square;
            }
        }
        private void LoadEDPosition()
        {
            // Implement Auto-save if preferred
            flowLayoutPanel1.Controls.Clear();
            SSPanel.Controls.Clear();
            SSCurrentLabel = null;
            SSLastNumLabel = null;
            ActiveTabier = null;
            Pieces.Clear();
            CapturedPieces.Clear();
            if (EDEnPassantSQ.Text != "")
            {
                Square square = GetSquare(EDEnPassantSQ.Text);
                if (square.Name.Substring(1, 1) == "3")
                {
                    EnPassantPawn = GetSquare(square.Name.Substring(0, 1) + 4).Piece;
                }
                else if (square.Name.Substring(1, 1) == "6")
                {
                    EnPassantPawn = GetSquare(square.Name.Substring(0, 1) + 5).Piece;
                }
            }
            if (String.IsNullOrWhiteSpace(EDStartNo.Text))
            {
                MoveCount = 0;
            }
            else
            {
                MoveCount = (sideToPlay == Piece.PieceSide.White ? 
                    (int.Parse(EDStartNo.Text) * 2 ) - 2: (int.Parse(EDStartNo.Text) * 2) - 1);
            }
            KingsideCastlingBlack = EDKSB.Checked;
            KingsideCastlingWhite = EDKSW.Checked;
            QueensideCastlingBlack = EDQSB.Checked;
            QueensideCastlingWhite = EDQSW.Checked;

            foreach (var item in Squares)
            {
                item.Piece = item.EDPiece;
                if (item.Piece != null)
                {
                    Pieces.Add(item.Piece);
                    item.Piece.Square = item;
                    if (item.Piece.Name == "White King")
                        WhiteKing = item.Piece;
                    else if (item.Piece.Name == "Black King")
                        BlackKing = item.Piece;
                }
            }
            UpdateArrow(true);
            LastMoveHighlighted = null;
            panel1.Refresh();
        }
        private void InvertPieces()
        {
            for (int i = 0; i < 32; i++)
            {
                Square square = Squares[i];
                Piece tempPiece = square.EDPiece;
                square.EDPiece = square.FlipSquare.EDPiece;
                square.FlipSquare.EDPiece = tempPiece;
            }
        }
        void EDPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (EDPieceSelected == null)
                return;
            Rectangle tempRect;
            if (e != null)
                tempRect = new Rectangle(EDPieceSelected.Location, EDPieceSelected.Image.Size);
            else
                tempRect = EDPanel.DisplayRectangle;
            Graphics g = Graphics.FromImage(EDImage);
            foreach (Square square in Squares)
                if (square.EDRectangle.IntersectsWith(tempRect))
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor : LightSquareColor, square.EDRectangle);
                    if (square.EDPiece != null)
                    {
                        int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                        int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                        int dX = midX - square.EDPiece.Image.Width / 2;
                        int dY = midY - square.EDPiece.Image.Height / 2;
                        square.EDPiece.Location = new Point(dX, dY);
                        g.DrawImageUnscaled(square.EDPiece.Image, dX, dY);
                    }
                }
            if (e != null)
            {
                Size midSize = new Size(EDPieceSelected.Image.Width / 2, EDPieceSelected.Image.Height / 2);
                g.DrawImageUnscaled(EDPieceSelected.Image, e.Location - midSize);
                EDPieceSelected.Location = e.Location - midSize; 
            }
        }
        void EDRefreshTimer_Tick(object sender, EventArgs e)
        {
            EDPanel_Paint(this, new PaintEventArgs(Graphics.FromImage(EDImage), 
                new Rectangle(new Point(0, 0), EDPanel.Size)));
        }
        void EDPanel_Paint(object sender, PaintEventArgs e)
        {
            EDPanel.CreateGraphics().DrawImageUnscaled(EDImage, new Point(0, 0));

            Graphics graphics = EDCordPanel.CreateGraphics();
            char tempChar = 'a', tempChar2 = 'A';
            Font SquareCordFont = new Font("Calibri", 8F, FontStyle.Regular);
            for (int i = 0; i < 8; i++, tempChar++, tempChar2++)
            {
                Square square = GetSquare((tempChar).ToString() + "1");
                int midX = (square.Rectangle.Right - square.Rectangle.Left) / 2
                    + square.Rectangle.Left;
                graphics.DrawString((tempChar2).ToString(), SquareCordFont,
                    Brushes.Black, new PointF(midX + 13, 3));
                graphics.DrawString((tempChar2).ToString(), SquareCordFont,
                    Brushes.Black, new PointF(midX + 13, EDCordPanel.Height - 15));
            }
            if (!isBoardFlipped)
            {
                tempChar = 'a';
                for (int i = 1; i < 9; i++)
                {
                    Square square = GetSquare((tempChar).ToString() + i);
                    int midY = (square.Rectangle.Bottom - square.Rectangle.Top) / 2
                        + square.Rectangle.Top;
                    graphics.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(3, midY + 13));
                    graphics.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(EDCordPanel.Right - 15, midY + 13));
                } 
            }
            else
            {
                tempChar = 'a';
                for (int i = 8; i > 0; i--)
                {
                    Square square = GetSquare((tempChar).ToString() + i);
                    int midY = (square.Rectangle.Bottom - square.Rectangle.Top) / 2
                        + square.Rectangle.Top;
                    graphics.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(3, midY + 13));
                    graphics.DrawString((i).ToString(), SquareCordFont,
                        Brushes.Black, new PointF(EDCordPanel.Right - 15, midY + 13));
                }
            }
        }
        private void FlipBoard()
        {
            for (int i = 0; i < 32; i++)
            {
                Square square = Squares[i];
                Rectangle tempRect = square.Rectangle;
                square.Rectangle = square.FlipSquare.Rectangle;
                square.FlipSquare.Rectangle = tempRect;
            }
            using (Graphics g = panel1.CreateGraphics())
            foreach (var square in Squares)
            {
                g.FillRectangle(square.Type == Square.SquareType.Dark ? DarkSquareColor :
                    LightSquareColor, square.Rectangle);
                if (square.Piece != null && square.Piece != PieceClicked)
                    PlacePiece(square.Piece, square); 
            }

            if (isInfiniteSearch)
                UpdateArrow(false);
            isBoardFlipped = !isBoardFlipped;
            InitializeSquareCordinates();

            //For Position edit panel
            for (int i = 0; i < 32; i++)
            {
                Square square = Squares[i];
                Rectangle tempRect = square.EDRectangle;
                square.EDRectangle = square.FlipSquare.EDRectangle;
                square.FlipSquare.EDRectangle = tempRect;
            }
        }
        void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!ShouldAnimate)
            {
                AnimationTimer.Stop();
                return;
            }
            InvalidatedSquares = new List<Square>();
            List<Piece> TempList = new List<Piece>();
            foreach (var item in AnimationList)
            {
                if (item.Stage == AnimationTask.AnimationStage.Completed)
                {
                    AnimationTimer.Stop();
                    IsAnimating = false;
                    HighlightCheckedKing();
                    if (ShouldHighlightLastMove && CurrentPosition.LastMovePlayed != null)
                        HighLightLastMove(CurrentPosition.LastMovePlayed);
                    if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0 && !isInfiniteSearch && !BestMoveArrow.Enabled)
                    {
                        List<Square> list = new List<Square>();
                        foreach (var line in CurrentPosition.Lines)
                        {
                            line.Enabled = true;
                            list.AddRange(line.Squares);
                        }
                        RefreshSquares(list, RedrawPerspective.None, null);
                    }
                    if (isInfiniteSearch)
                    {
                        ArrowTimer.Enabled = true;
                        ArrowTimer.Start(); 
                    }
                    return;
                }
                TempList.Add(item.Move.PieceMoving);
                Rectangle tempRect = new Rectangle(item.Move.PieceMoving.Location, 
                    item.Move.PieceMoving.Image.Size);
                foreach (Square square in Squares)
                {
                    if (square.Rectangle.IntersectsWith(tempRect))
                    {
                        InvalidatedSquares.Add(square);
                    }   
                }
            }

            foreach (var square in InvalidatedSquares)
            {
                using (Graphics g = panel1.CreateGraphics())
                {
                    g.FillRectangle(square.Type == Square.SquareType.Dark
                        ? DarkSquareColor : LightSquareColor, square.Rectangle);
                    if (square.Piece != null && !TempList.Contains(square.Piece))
                    {
                        g.DrawImageUnscaled(square.Piece.Image,
                            square.Piece.Location);
                    }
                }
            }

            foreach (var item in AnimationList)
            {
                switch (item.Stage)
                {
                    case AnimationTask.AnimationStage.First:
                        graphics.DrawImageUnscaled(item.Move.PieceMoving.Image, item.MidPoints[0]);
                        item.Move.PieceMoving.Location = item.MidPoints[0];
                        item.Stage = AnimationTask.AnimationStage.Second;
                        break;
                    case AnimationTask.AnimationStage.Second:
                        graphics.DrawImageUnscaled(item.Move.PieceMoving.Image, item.MidPoints[1]);
                        item.Move.PieceMoving.Location = item.MidPoints[1];
                        item.Stage = AnimationTask.AnimationStage.Third;
                        break;
                    case AnimationTask.AnimationStage.Third:
                        graphics.DrawImageUnscaled(item.Move.PieceMoving.Image, item.MidPoints[2]);
                        item.Move.PieceMoving.Location = item.MidPoints[2];
                        item.Stage = AnimationTask.AnimationStage.Fourth;
                        break;
                    case AnimationTask.AnimationStage.Fourth:
                        graphics.DrawImageUnscaled(item.Move.PieceMoving.Image, item.StoppingPoint);
                        item.Move.PieceMoving.Location = item.StoppingPoint;
                        item.Stage = AnimationTask.AnimationStage.Completed;
                        break;
                    default:
                        AnimationTimer.Stop();
                        AnimationTimer.Enabled = false;
                        break;
                }
            }
        }
        private void SetGameDetails(IPlayer player1, IPlayer player2, GameDetails gd)
        {
            gameDetails = gd;
            gd.isUserGame = true;
            WhiteTime.TimeLeft = timeControl.MainTime;
            BlackTime.TimeLeft = timeControl.MainTime;
            WhiteTime.Increment = timeControl.MainIncrement;
            BlackTime.Increment = timeControl.MainIncrement;

            DateTime dt = DateTime.Now;
            gd.Date = dt.Year + "." + dt.Month + "." + dt.Day;
            gd.RegularDateString = dt.Year + "/" + dt.Month + "/" + dt.Day;
            if (player1 == null || player2 == null)
            {
                ModeOfPlay = PlayMode.EditPosition;
                gd.WhitePlayer = CurrentUser;
                gd.BlackPlayer = CurrentUser;
                foreach (var engine in LoadedEngines)
                    if (engine._UCI_AnalyseMode)
                        engine.Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value true");
                WhiteTime.Player = CurrentUser;
                BlackTime.Player = CurrentUser;
                WhiteClockLabel.Tag = null;
                BlackClockLabel.Tag = null;
            }

            else if (player1 is User && player2 is User)
            {
                if (UserList.Contains(player1 as User) && UserList.Contains(player2 as User))
                {
                    ModeOfPlay = PlayMode.TwoPlayer;
                    ShouldClockTick = false;
                    gd.WhitePlayer = player1;
                    gd.BlackPlayer = player2;
                    player1.Opponent = player2;
                    player2.Opponent = player1;
                    CurrentPlayer = player1;
                    player1.Side = Piece.PieceSide.White;
                    player2.Side = Piece.PieceSide.Black;
                    player1.Time = WhiteTime;
                    player2.Time = BlackTime;
                    WhiteTime.Player = player1;
                    BlackTime.Player = player2;
                    WhiteClockLabel.Tag = WhiteTime.Player;
                    BlackClockLabel.Tag = BlackTime.Player;
                    foreach(var engine in LoadedEngines)
                        if (engine._UCI_AnalyseMode)
                            engine.Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value true");
                }
                else
                {
                    MessageBox.Show("Error setting game details 01");
                    return;
                }
            }
            else if (player1 is Engine && player2 is Engine)
            {
                if ((player1 as Engine).isUciEngine && (player2 as Engine).isUciEngine)
                {
                    ModeOfPlay = PlayMode.EngineVsEngine;
                    gd.WhitePlayer = player1;
                    gd.BlackPlayer = player2;
                    player1.Opponent = player2;
                    player2.Opponent = player1;
                    player1.Side = Piece.PieceSide.White;
                    player2.Side = Piece.PieceSide.Black;
                    (player1 as Engine).MainLine.Evaluation = "";
                    (player2 as Engine).MainLine.Evaluation = "";
                    (player1 as Engine).Process.StandardInput.WriteLine("ucinewgame");
                    (player2 as Engine).Process.StandardInput.WriteLine("ucinewgame");
                    player1.Time = WhiteTime;
                    player2.Time = BlackTime;
                    WhiteTime.Player = player1;
                    BlackTime.Player = player2;
                    WhiteClockLabel.Tag = WhiteTime.Player;
                    BlackClockLabel.Tag = BlackTime.Player;

                    if ((player1 as Engine)._UCI_AnalyseMode)
                        (player1 as Engine).Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value false");
                    if ((player2 as Engine)._UCI_AnalyseMode)
                        (player2 as Engine).Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value false");

                    CurrentPlayer = sideToPlay == Piece.PieceSide.White ? player1 : player2;
                    SendPositionToEngine((sideToPlay == Piece.PieceSide.White ? player1: player2) as Engine, "");
                    ((sideToPlay == Piece.PieceSide.White ? player1 : player2) as Engine).ShouldIgnore = false;
                    ((sideToPlay == Piece.PieceSide.White ? player1: player2) as Engine).Process.StandardInput.WriteLine
                        ("go " + AddTimeInfo());
                    (sideToPlay == Piece.PieceSide.White ? player1: player2).Time.Differential = DateTime.Now;
                    (sideToPlay == Piece.PieceSide.White ? player1: player2).Time.isTicking = true;
                    ((sideToPlay == Piece.PieceSide.White ? player1 : player2) as Engine).isAnalyzing = true;
                    (sideToPlay != Piece.PieceSide.White ? player1 : player2).Time.isTicking = false;
                    ShouldClockTick = true;
                    ClockTimer.Start();
                    isEngineMatchInProgress = true;
                }
                else
                {
                    MessageBox.Show("Error setting game details 02");
                    return;
                }
            }
            else    //User vs Engine
            {
                PlayMode pm = ModeOfPlay;
                ModeOfPlay = PlayMode.SinglePlayer;
                User user = player1 is User ? player1 as User : player2 as User;
                Engine engine = player1 is User ? player2 as Engine : player1 as Engine;

                if (engine._UCI_AnalyseMode)
                    engine.Process.StandardInput.WriteLine("setoption name UCI_AnalyseMode value false");
                engine.MainLine.Evaluation = "";
                engine.Process.StandardInput.WriteLine("ucinewgame");
                if (UserList.Contains(user) && engine.isUciEngine)
                {
                    ShouldClockTick = false;
                    gd.WhitePlayer = player1;
                    gd.BlackPlayer = player2;
                    CurrentUser = user;
                    user.Side = user == player1 ? Piece.PieceSide.White : Piece.PieceSide.Black;
                    engine.Side = engine == player1 ? Piece.PieceSide.White : Piece.PieceSide.Black;
                    player1.Time = WhiteTime;
                    player2.Time = BlackTime;
                    WhiteTime.Player = player1;
                    BlackTime.Player = player2;
                    user.Time.TimeLeft += timeControl.HumanBonusTime;
                    user.Time.Increment += timeControl.HumanBonusIncrement;
                    WhiteClockLabel.Tag = WhiteTime.Player;
                    BlackClockLabel.Tag = BlackTime.Player;
                    engine.Opponent = user;
                    user.Opponent = engine;
                    CurrentPlayer = user == player1 ? user as IPlayer : engine as IPlayer;
                    if (engine.Side == sideToPlay)
                    {
                        SendPositionToEngine(engine, "");
                        engine.Process.StandardInput.WriteLine("go "
                            + AddTimeInfo());
                        engine.ShouldIgnore = false;
                        player1.Time.Differential = DateTime.Now;
                        ShouldClockTick = true;
                        engine.isAnalyzing = true;
                        isTimePaused = false;
                        player1.Time.isTicking = true;
                        ClockTimer.Start();

                        FlipBoard();
                    }
                }
                else
                {
                    ModeOfPlay = pm;
                    MessageBox.Show("Error setting game details 03");
                }
            }
            WhiteClockLabel.Text = WhiteTime.ToString(true);
            BlackClockLabel.Text = BlackTime.ToString(true);
        }
        private void ShowGameDetails()
        {
            Name_EloLabel.Text = "";
            if (gameDetails.WhitePlayer.Name != "")
            {
                Name_EloLabel.Text += gameDetails.WhitePlayer.Name + " ";
                Name_EloLabel.Text += gameDetails.WhitePlayer.Rating > 0 ? "(" + gameDetails.WhitePlayer.Rating + ")" : "";
            }
            Name_EloLabel.Text += " - ";
            if (gameDetails.BlackPlayer.Name != "")
            {
                Name_EloLabel.Text += gameDetails.BlackPlayer.Name + " ";
                Name_EloLabel.Text += gameDetails.BlackPlayer.Rating > 0 ? "(" + gameDetails.BlackPlayer.Rating + ")" : "";
            }
            Name_EloLabel.Text += " " +gameDetails.GetResultString();

            if (gameDetails.Event != "?" && !String.IsNullOrWhiteSpace(gameDetails.Event))
            {
                Event_DateLabel.Text = gameDetails.Event == "?" ? "" : gameDetails.Event;
                Event_DateLabel.Text += gameDetails.Round > 0 ? (" (Round " + gameDetails.Round + ")") : "";
                Event_DateLabel.Text += gameDetails.Date != "?" ? (", " + gameDetails.Date.Substring(0, 4)) : ""; 
            }

            Name_EloLabel.Location = new Point(tabPage1.Width / 2 - Name_EloLabel.Width / 2, flowLayoutPanel1.Top - 75);
            Event_DateLabel.Location = new Point(tabPage1.Width / 2 - Event_DateLabel.Width / 2, flowLayoutPanel1.Top - 50);
            OpeningLabel.Location = new Point(tabPage1.Width / 2 - OpeningLabel.Width / 2, flowLayoutPanel1.Top - 30);

            SSWhiteV.Text = gameDetails.WhitePlayer.Name != "?" ? gameDetails.WhitePlayer.Name : "";
            SSWhiteEloV.Text = gameDetails.WhitePlayer.Rating > 0 ? gameDetails.WhitePlayer.Rating.ToString() : "";

            SSBlackV.Text = gameDetails.BlackPlayer.Name != "?" ? gameDetails.BlackPlayer.Name : "";
            SSBlackEloV.Text = gameDetails.BlackPlayer.Rating > 0 ? gameDetails.BlackPlayer.Rating.ToString() : "";

            SSEventV.Text = gameDetails.Event != "?" ? gameDetails.Event : "";
            SSDateV.Text = gameDetails.Date != "?" ? gameDetails.RegularDateString : "";
            SSResultV.Text = gameDetails.GetResultString();
            SSRoundV.Text = gameDetails.Round > 0 ? gameDetails.Round.ToString() : "";

            toolTip1.SetToolTip(Name_EloLabel, Name_EloLabel.Text);
            toolTip1.SetToolTip(Event_DateLabel, Event_DateLabel.Text);
            toolTip1.SetToolTip(OpeningLabel, OpeningLabel.Text);
            toolTip1.SetToolTip(SSEventV, SSEventV.Text);
            toolTip1.SetToolTip(SSWhiteV, SSWhiteV.Text);
            toolTip1.SetToolTip(SSBlackV, SSBlackV.Text);
            toolTip1.SetToolTip(SSWhiteEloV, SSWhiteEloV.Text);
            toolTip1.SetToolTip(SSBlackEloV, SSBlackEloV.Text);
            toolTip1.SetToolTip(SSRoundV, SSRoundV.Text);
            toolTip1.SetToolTip(SSDateV, SSDateV.Text);
            toolTip1.SetToolTip(SSResultV, SSResultV.Text);

            int x = int.Parse(SSWhiteV.AccessibleName) - SSWhiteV.Width;
            if (x > 0)
                SSWhiteV.Text = SSWhiteV.Text.PadRight(SSWhiteV.Text.Length + x / 5);
            x = int.Parse(SSBlackV.AccessibleName) - SSBlackV.Width;
            if (x > 0)
                SSBlackV.Text = SSBlackV.Text.PadRight(SSBlackV.Text.Length + x / 5);
            x = int.Parse(SSEventV.AccessibleName) - SSEventV.Width;
            if (x > 0)
                SSEventV.Text = SSEventV.Text.PadRight(SSEventV.Text.Length + x / 5);
            x = int.Parse(SSDateV.AccessibleName) - SSDateV.Width;
            if (x > 0)
                SSDateV.Text = SSDateV.Text.PadRight(SSDateV.Text.Length + x / 5);
            x = int.Parse(SSResultV.AccessibleName) - SSResultV.Width;
            if (x > 0)
                SSResultV.Text = SSResultV.Text.PadRight(SSResultV.Text.Length + x / 5);
            x = int.Parse(SSRoundV.AccessibleName) - SSRoundV.Width;
            if (x > 0)
                SSRoundV.Text = SSRoundV.Text.PadRight(SSRoundV.Text.Length + x / 5);
            x = int.Parse(SSWhiteEloV.AccessibleName) - SSWhiteEloV.Width;
            if (x > 0)
                SSWhiteEloV.Text = SSWhiteEloV.Text.PadRight(SSWhiteEloV.Text.Length + x / 5);
            x = int.Parse(SSBlackEloV.AccessibleName) - SSBlackEloV.Width;
            if (x > 0)
                SSBlackEloV.Text = SSBlackEloV.Text.PadRight(SSBlackEloV.Text.Length + x / 5);
        }
        private void TapClock()
        {
            ShouldClockTick = true;

            if (PausePending)
            {
                PauseClocks();
                PausePending = false;
                pauseClocksToolStripMenuItem.Text = "Restart Clocks";
            }
            else
            {
                isTimePaused = false;
                pauseClocksToolStripMenuItem.Text = "Pause Clocks";
            }

            if (CurrentPosition.LastMovePlayed.MoveNo == timeControl.TCMoveNumber &&
                timeControl.TCMoveNumber != 0)
            {
                CurrentPlayer.Time.TimeLeft += timeControl.TCTime;
            }

            CurrentPlayer.Time.TimeLeft +=
                    new TimeSpan(0, 0, 0, CurrentPlayer.Time.Increment, 10 * CurrentPlayer.Time.AnimationCompCount);
            CurrentPlayer.Time.isTicking = false;
            CurrentPlayer.Time.ClockLabel.Text = CurrentPlayer.Time.ToString(true);
            CurrentPlayer.Opponent.Time.isTicking = true;
            CurrentPlayer.Opponent.Time.Differential = DateTime.Now;

            if (CurrentPlayer is Engine)
            {
                Engine engine = CurrentPlayer as Engine;
                engine.ShouldIgnore = true;
                //SendPositionToEngine(engine, engine.PonderString);
                //engine.Process.StandardInput.WriteLine("go ponder " +AddTimeInfo());
                //engine.Process.StandardInput.WriteLine("go infinite");
            }
            if (CurrentPlayer.Opponent is Engine)
            {
                Engine engine = CurrentPlayer.Opponent as Engine;
                //isSearchInProgress = true;
                if (CurrentPosition.LastMovePlayed.UCINotation == engine.PonderString)
                {
                    //engine.isBusy = false;
                    //engine.Process.StandardInput.WriteLine("ponderhit");
                    SendPositionToEngine(engine, "");
                    engine.ShouldIgnore = false;
                    engine.Process.StandardInput.WriteLine("go " + AddTimeInfo());
                    engine.isAnalyzing = true;
                }
                else
                {
                    SendPositionToEngine(engine, "");
                    engine.ShouldIgnore = false;
                    engine.Process.StandardInput.WriteLine("go " + AddTimeInfo());
                    engine.isAnalyzing = true;
                }
            }
            CurrentPlayer = CurrentPlayer.Opponent;
            HandleEtiquette();
        }
        private void HandleEtiquette()
        {
            if (ModeOfPlay == PlayMode.EditPosition || ModeOfPlay == PlayMode.TwoPlayer || !ShouldUseEtiquette
                || IsDraw)
                return;
            if (gameDetails.Result != GameDetails.Outcome.NotAvailable)
                return;

            if (CurrentPlayer is Engine)
            {
                Engine engine = CurrentPlayer as Engine;
                if (engine.MainLine.Evaluation.Contains("#"))
                {
                    if (engine.MainLine.Evaluation.Contains("-") && engine.Side == Piece.PieceSide.White)
                        EndGame(GameDetails.Outcome.BlackWin);
                    else if (!engine.MainLine.Evaluation.Contains("-") && engine.Side == Piece.PieceSide.Black)
                        EndGame(GameDetails.Outcome.WhiteWin);
                }
                else
                {
                    decimal d;
                    if (!decimal.TryParse(engine.MainLine.Evaluation, out d))
                        return;
                    if ((Math.Abs(d) >= 20 && engine.Opponent is User ) ||
                        Math.Abs(d) >= 10 && engine.Opponent is Engine)
                    {
                        if (d > 0 && engine.Side == Piece.PieceSide.Black)
                            EndGame(GameDetails.Outcome.WhiteWin);
                        else if (d < 0 && engine.Side == Piece.PieceSide.White)
                            EndGame(GameDetails.Outcome.BlackWin);
                    }
                }
                if (gameDetails.Result == GameDetails.Outcome.NotAvailable)
                {
                    if ( isEngineMatchInProgress && engine.Opponent is Engine)
                    {
                        decimal d1, d2;
                        if (!decimal.TryParse(engine.MainLine.Evaluation, out d1) ||
                            !decimal.TryParse((engine.Opponent as Engine).MainLine.Evaluation, out d2))
                            return;
                        if (Pieces.Count <= 8 || MoveCount > 100)
                        {
                            if (Math.Abs(d1) < (decimal)0.2 && Math.Abs(d2) < (decimal)0.2)
                                EndGame(GameDetails.Outcome.Draw);
                        }
                        else
                        {
                            if (d1 == (decimal)0.00 && d2 == (decimal)0.00 && MoveCount > 60)
                                EndGame(GameDetails.Outcome.Draw);
                        }
                    }
                    else if (engine.Opponent is User)
                    {
                        decimal d;
                        if (!decimal.TryParse(engine.MainLine.Evaluation, out d))
                            return;
                        if (Pieces.Count <= 8 || MoveCount > 140)
                        {
                            if ((d < 0 && engine.Side == Piece.PieceSide.White) || 
                                d > 0 && engine.Side == Piece.PieceSide.Black)
                                OfferDraw(CurrentPlayer, engine.Opponent as User);
                        } 
                    }
                }
            }
        }
        private void OfferDraw(IPlayer player, User user)
        {
            DialogResult dr = MessageBox.Show(user.Name + ", do you want a draw?", "Draw Offer", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                ShouldClockTick = false;
                (player as Engine).ShouldIgnore = true;
                ModeOfPlay = PlayMode.EditPosition;
                gameDetails.Result = GameDetails.Outcome.Draw;
                foreach (var item in LoadedEngines)
                    if (item.isUciEngine)
                    {
                        item.Process.StandardInput.WriteLine("stop");
                        item.isAnalyzing = false;
                    }
                MessageBox.Show("Game Over! \nDraw agreed");
            }
        }
        private void EndGame(GameDetails.Outcome g_o)
        {
            ShouldClockTick = false;
            gameDetails.Result = g_o;
            OnGameEnding();
            ModeOfPlay = PlayMode.EditPosition;
            isEngineMatchInProgress = false;
            if (CurrentPlayer is Engine)
            {
                Engine engine = CurrentPlayer as Engine;
                engine.ShouldIgnore = true;
                foreach (var item in LoadedEngines)
                    if (item.isUciEngine)
                    {
                        item.Process.StandardInput.WriteLine("stop");
                        item.isAnalyzing = false;
                    }
            }
            ShowGameDetails();
            switch (g_o)
            {
                case GameDetails.Outcome.WhiteWin:
                    MessageBox.Show("Game Over! \nBlack Resigns");
                    break;
                case GameDetails.Outcome.Draw:
                    MessageBox.Show("Game Over! \nDraw agreed");
                    break;
                case GameDetails.Outcome.BlackWin:
                    MessageBox.Show("Game Over! \nWhite Resigns");
                    break;
                case GameDetails.Outcome.NotAvailable:
                    break;
                default:
                    break;
            }
        }
        private void OnGameEnding()
        {
            WhiteTime.isTicking = false;
            BlackTime.isTicking = false;
            CurrentPlayer = null;

            if (settings.appearance.SaveUserArrows == "Save with position")
                if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0)
                {
                    List<Square> list = new List<Square>();
                    foreach (var item in CurrentPosition.Lines)
                    {
                        item.Enabled = true;
                        list.AddRange(item.Squares);
                    }
                    RefreshSquares(list, RedrawPerspective.Arrow, null);
                }

            if (FirstEngine != null)
                FirstEngine.isAnalyzing = false;
            if (SecondEngine != null)
                SecondEngine.isAnalyzing = false;
            if (!gameDetails.isGameRated)
                return;
            if (gameDetails.WhitePlayer is User)
            {
                User user = gameDetails.WhitePlayer as User;
                if (user.Opponent.Rating == 0)
                    return;
                user.PRGameCount++;
                user.PRTotalOppRating += user.Opponent.Rating;
                if (gameDetails.Result == GameDetails.Outcome.WhiteWin)
                    user.PRWinLossDiff++;
                else if (gameDetails.Result == GameDetails.Outcome.BlackWin)
                    user.PRWinLossDiff--;
                user.ComputeRating();
            }
            if (gameDetails.BlackPlayer is User)
            {
                User user = gameDetails.BlackPlayer as User;
                if (user.Opponent.Rating == 0)
                    return;
                user.PRGameCount++;
                user.PRTotalOppRating += user.Opponent.Rating;
                if (gameDetails.Result == GameDetails.Outcome.WhiteWin)
                    user.PRWinLossDiff++;
                else if (gameDetails.Result == GameDetails.Outcome.BlackWin)
                    user.PRWinLossDiff--;
                user.ComputeRating();
            }
        }
        void ClockTimer_Tick(object sender, EventArgs e)
        {
            if (!ShouldClockTick || ModeOfPlay == PlayMode.EditPosition || isTimePaused)
                return;
            if (WhiteTime.isTicking)
            {
                if (WhiteTime.TimeLeft <= TimeSpan.Zero && settings.behaviour.TimeForfeit)
                {
                    ShouldClockTick = false;
                    WhiteTime.TimeLeft = TimeSpan.Zero;
                    ModeOfPlay = PlayMode.EditPosition;
                    gameDetails.Result = GameDetails.Outcome.WhiteWin;
                    OnGameEnding();
                    ShowGameDetails();
                    WhiteClockLabel.Text = WhiteTime.ToString();
                    MessageBox.Show("Time Up! Black Wins");
                    if (PieceClicked != null)
                    {
                        Rectangle tempRect = 
                            new Rectangle(PieceClicked.Location, PieceClicked.Image.Size);
                        foreach (var square in Squares)
                        {
                            using (Graphics g = panel1.CreateGraphics())
                            if (square.Rectangle.IntersectsWith(tempRect))
                            {
                                g.FillRectangle(square.Type == Square.SquareType.Dark 
                                    ? DarkSquareColor : LightSquareColor, square.Rectangle);
                                if (square.Piece != null && square.Piece != PieceClicked)
                                    PlacePiece(square.Piece, square);
                            } 
                        }
                        PlacePiece(PieceClicked, PieceClicked.Square);
                        PieceClicked = null;
                    }
                    return;
                }
                if (ShouldClockTick)
                {    
                    WhiteTime.TimeLeft -= 
                        (DateTime.Now - WhiteTime.Differential - new TimeSpan(0, 0, 0, 0, 15));
                    //if (WhiteTime.TimeLeft <= new TimeSpan(0, 0, 1))
                        //ClockTimer.Interval = 100;
                    if (WhiteTime.TimeLeft >= TimeSpan.Zero)
                        WhiteClockLabel.Text = WhiteTime.ToString();
                    WhiteTime.Differential = DateTime.Now;
                }
            }
            else if (BlackTime.isTicking)
            {
                if (BlackTime.TimeLeft <= TimeSpan.Zero && settings.behaviour.TimeForfeit)
                {
                    ShouldClockTick = false;
                    BlackTime.TimeLeft = TimeSpan.Zero;
                    BlackClockLabel.Text = BlackTime.ToString();
                    ModeOfPlay = PlayMode.EditPosition;
                    gameDetails.Result = GameDetails.Outcome.WhiteWin;
                    OnGameEnding();
                    ShowGameDetails();
                    MessageBox.Show("Time Up! White Wins");
                    if (PieceClicked != null)
                    {
                        Rectangle tempRect =
                            new Rectangle(PieceClicked.Location, PieceClicked.Image.Size);
                        foreach (var square in Squares)
                        {
                            using (Graphics g = panel1.CreateGraphics())
                                if (square.Rectangle.IntersectsWith(tempRect))
                                {
                                    g.FillRectangle(square.Type == Square.SquareType.Dark
                                        ? DarkSquareColor : LightSquareColor, square.Rectangle);
                                    if (square.Piece != null && square.Piece != PieceClicked)
                                        PlacePiece(square.Piece, square);
                                }
                        }
                        PlacePiece(PieceClicked, PieceClicked.Square);
                        PieceClicked = null;
                    }
                    return;
                }
                if (ShouldClockTick)
                {
                    BlackTime.TimeLeft -= 
                        (DateTime.Now - BlackTime.Differential - new TimeSpan(0, 0, 0, 0, 15));
                    //if (BlackTime.TimeLeft <= new TimeSpan(0, 0, 1))
                       // ClockTimer.Interval = 100;
                    BlackTime.Differential = DateTime.Now;
                    if (BlackTime.TimeLeft >= TimeSpan.Zero)
                        BlackClockLabel.Text = BlackTime.ToString();
                }
            }
        }
        private void HighLightLastMove(Move move)
        {
            using (Graphics g = panel1.CreateGraphics())
            {
                if (LastMoveHighlighted != null)
                {
                    var list = new List<Square>();
                    list.Add(LastMoveHighlighted.OriginalSquare);
                    list.Add(LastMoveHighlighted.DestSquare);
                    RefreshSquares(list, RedrawPerspective.LastMoveHL, null);
                }

                if (move != null)
                {
                    if (settings.appearance.LastMoveHLType == "Shade")
                    {
                        graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, LastMoveHLColor)),
                                        move.OriginalSquare.Rectangle);
                    }
                    else if (settings.appearance.LastMoveHLType == "Border")
                    {
                        graphics.DrawRectangle(new Pen(LastMoveHLColor, 4),
                        new Rectangle(move.OriginalSquare.Rectangle.Location + new Size(2, 2),
                            move.OriginalSquare.Rectangle.Size - new Size(4, 4)));
                    }
                    if (move.OriginalSquare.Piece != null)
                        PlacePiece(move.OriginalSquare.Piece, move.OriginalSquare);

                    if (settings.appearance.LastMoveHLType == "Shade")
                    {
                        graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, LastMoveHLColor)),
                                        move.DestSquare.Rectangle);
                    }
                    else if (settings.appearance.LastMoveHLType == "Border")
                    {
                        graphics.DrawRectangle(new Pen(LastMoveHLColor, 4),
                        new Rectangle(move.DestSquare.Rectangle.Location + new Size(2, 2),
                            move.DestSquare.Rectangle.Size - new Size(4, 4)));
                    }
                    if (move.DestSquare.Piece != null)
                        PlacePiece(move.DestSquare.Piece, move.DestSquare); 
                }

                LastMoveHighlighted = move;
            }
        }
        private void HighlightCheckedKing()
        {
            if (!IsWhiteInCheck && !IsBlackInCheck)
            {
                if (CheckedKingHLSquare != null)
                {
                    List<Square> list = new List<Square>();
                    list.Add(CheckedKingHLSquare);
                    RefreshSquares(list, RedrawPerspective.CheckedKingHL, null);
                    CheckedKingHLSquare = null;
                }
                return;
            }
            if (!ShouldHighlightCheckedKing)
                return;

            CheckedKingHLSquare = (IsWhiteInCheck ? WhiteKing.Square : BlackKing.Square);
            using (Graphics g = panel1.CreateGraphics())
            {
                g.FillRectangle(CheckedKingHLSquare.Type == Square.SquareType.Light ?
                    LightSquareColor : DarkSquareColor, CheckedKingHLSquare.Rectangle);

                Bitmap bmp = new Bitmap(Properties.Resources.KingInCheckRed, CheckedKingHLSquare.Rectangle.Size);
                TextureBrush tb = new TextureBrush(bmp);
                if (settings.appearance.KICHLType == "Glow")
                {
                    g.DrawImage(bmp, CheckedKingHLSquare.Rectangle.Location); 
                }
                else if (settings.appearance.KICHLType == "Border")
                {
                    g.DrawRectangle(new Pen(Color.Red, 4), 
                        new Rectangle(CheckedKingHLSquare.Rectangle.Location + new Size(2, 2), 
                            CheckedKingHLSquare.Rectangle.Size - new Size(4, 4)));
                }
                g.DrawImageUnscaled(CheckedKingHLSquare.Piece.Image, CheckedKingHLSquare.Piece.Location);
            }

        }
        private void RefreshSquares(List<Square> tempList, RedrawPerspective rdp, Piece piece)
        {
            panel1.SuspendLayout();
            foreach (var item in tempList)
                graphics.FillRectangle(item.Type == Square.SquareType.Light ?
                    LightSquareColor : DarkSquareColor, item.Rectangle);
            
            if (rdp != RedrawPerspective.CheckedKingHL && ShouldHighlightCheckedKing &&
                CheckedKingHLSquare != null && tempList.Contains(CheckedKingHLSquare))
            {
                Bitmap bmp = new Bitmap(Properties.Resources.KingInCheckRed, CheckedKingHLSquare.Rectangle.Size);
                TextureBrush tb = new TextureBrush(bmp);
                if (settings.appearance.KICHLType == "Glow")
                {
                    graphics.DrawImage(bmp, CheckedKingHLSquare.Rectangle.Location);
                }
                else if (settings.appearance.KICHLType == "Border")
                {
                    graphics.DrawRectangle(new Pen(Color.Red, 4),
                        new Rectangle(CheckedKingHLSquare.Rectangle.Location + new Size(2, 2),
                            CheckedKingHLSquare.Rectangle.Size - new Size(4, 4)));
                }
            }

            if (rdp != RedrawPerspective.UserArrows && ShouldDrawUserArrows && CurrentPosition != null &&
                settings.appearance.UserArrowSquareHLType == "Shade")
            {
                foreach (var item in CurrentPosition.Lines)
                {
                    if (item.Pen == null)
                    {
                        if (item.Color == null)
                            continue;
                        item.Pen = new Pen(new SolidBrush(item.Color), 8F);
                        System.Drawing.Drawing2D.AdjustableArrowCap aac =
                            new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                        aac.MiddleInset = 2;
                        item.Pen.CustomEndCap = aac;
                    }
                    if (!item.Enabled)
                        continue;
                    if (item.StartingSquare == item.StoppingSquare && tempList.Contains(item.StartingSquare))
                        graphics.FillRectangle(item.Pen.Brush, item.StartingSquare.Rectangle);
                }
            }

            if (CurrentPosition != null && rdp != RedrawPerspective.LastMoveHL  && rdp != RedrawPerspective.LegalMoveHL && 
                CurrentPosition.LastMovePlayed != null && ShouldHighlightLastMove && LastMoveHighlighted != null
                && (!ShouldHLLegalSquares || !isHighlighted))
            {
                if (tempList.Contains(LastMoveHighlighted.OriginalSquare))
                {
                    if (settings.appearance.LastMoveHLType == "Shade")
                    {
                        graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, LastMoveHLColor)),
                                        LastMoveHighlighted.OriginalSquare.Rectangle); 
                    }
                    else if (settings.appearance.LastMoveHLType == "Border")
                    {
                        graphics.DrawRectangle(new Pen(LastMoveHLColor, 4), 
                        new Rectangle(LastMoveHighlighted.OriginalSquare.Rectangle.Location + new Size(2, 2),
                            LastMoveHighlighted.OriginalSquare.Rectangle.Size - new Size(4, 4)));
                    }
                }

                if (tempList.Contains(LastMoveHighlighted.DestSquare))
                {
                    if (settings.appearance.LastMoveHLType == "Shade")
                    {
                        graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, LastMoveHLColor)),
                                        LastMoveHighlighted.DestSquare.Rectangle);
                    }
                    else if (settings.appearance.LastMoveHLType == "Border")
                    {
                        graphics.DrawRectangle(new Pen(LastMoveHLColor, 4),
                        new Rectangle(LastMoveHighlighted.DestSquare.Rectangle.Location + new Size(2, 2),
                            LastMoveHighlighted.DestSquare.Rectangle.Size - new Size(4, 4)));
                    }
                }
            }

            if (rdp != RedrawPerspective.LegalMoveHL && ShouldHLLegalSquares && isHighlighted && CurrentPosition != null)
            {
                foreach (var item in tempList)
                {
                    if (HighlightedSquares.Contains(item))
                        HighlightSquare(item);
                }
            }

            foreach (var item in tempList)
                if (item.Piece != null && item.Piece != piece)
                    PlacePiece(item.Piece, item);

            if (rdp != RedrawPerspective.Arrow && BestMoveArrow.Enabled && BestMoveArrow.isShowing)
            {
                foreach (var item in tempList)
                if (BestMoveArrow.Squares.Contains(item))
                {
                    Point x = new Point(BestMoveArrow.StartingSquare.Rectangle.Height / 2,
                    BestMoveArrow.StartingSquare.Rectangle.Width / 2)
                    + new Size(BestMoveArrow.StartingSquare.Rectangle.Location);
                    Point y = new Point(BestMoveArrow.StoppingSquare.Rectangle.Height / 2,
                        BestMoveArrow.StoppingSquare.Rectangle.Width / 2)
                        + new Size(BestMoveArrow.StoppingSquare.Rectangle.Location);
                    panel1.CreateGraphics().DrawLine(BestMoveArrow.Pen, x, y);
                    BestMoveArrow.isShowing = true;
                    break;
                }
            }

            if (CurrentPosition != null && rdp != RedrawPerspective.UserArrows && ShouldDrawUserArrows)
            {
                foreach (var item in CurrentPosition.Lines)
                {
                    if (!item.Enabled)
                        continue;
                    bool PASS = false;
                    foreach (var square in tempList)
                    {
                        if (item.Squares.Contains(square))
                        {
                            PASS = true;
                            break;
                        }
                    }
                    if (!PASS)
                        continue;

                    if (item.Pen == null)
                    {
                        item.Pen = new Pen(new SolidBrush(item.Color), 8F);
                        System.Drawing.Drawing2D.AdjustableArrowCap aac =
                        new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                        aac.MiddleInset = 2;
                        item.Pen.CustomEndCap = aac;
                    }

                    if (item.StartingSquare != item.StoppingSquare)
                    {
                        Point x = new Point(item.StartingSquare.Rectangle.Height / 2,
                        item.StartingSquare.Rectangle.Width / 2)
                        + new Size(item.StartingSquare.Rectangle.Location);
                        Point y = new Point(item.StoppingSquare.Rectangle.Height / 2,
                            item.StoppingSquare.Rectangle.Width / 2)
                            + new Size(item.StoppingSquare.Rectangle.Location);
                        panel1.CreateGraphics().DrawLine(item.Pen, x, y);
                        item.isShowing = true;
                    }
                    else if (settings.appearance.UserArrowSquareHLType == "Circle")
                    {
                        graphics.DrawEllipse(new Pen(item.Color, 3),
                            new Rectangle(item.StartingSquare.Rectangle.Location + new Size(1, 1),
                                item.StartingSquare.Rectangle.Size - new Size(3, 3)));
                    }
                }
            }
            panel1.ResumeLayout();
        }
        private void FillCircle(Brush brush, Point point, int radius)
        {
            panel1.CreateGraphics().FillEllipse(brush, point.X - radius,
                point.Y - radius, radius + radius, radius + radius);
        }
        private void FillCircle(Brush brush, Rectangle rect, int radius)
        {
            int midX = rect.Width / 2;
            int midY = rect.Height / 2;
            FillCircle(brush, new Point(rect.X + midX, rect.Y + midY), radius);
        }
        [Serializable]
        public class Clock
        {
            public Clock()
            {
                Differential = DateTime.MinValue;
            }
            public TimeSpan TimeLeft { get; set; }
            public bool isTicking { get; set; }
            public int Increment { get; set; }
            public DateTime Differential { get; set; }
            public int AnimationCompCount { get; set; }
            [NonSerialized]
            Label clockLabel;
            public Label ClockLabel { get { return clockLabel; } set { clockLabel = value; } }
            public IPlayer Player { get; set; }

            String buffer = "";
            public override string ToString()
            {
                if (TimeLeft < TimeSpan.Zero)
                    return buffer;
                if (!buffer.Contains(':'))
                    buffer = TimeLeft.Hours.ToString() + ":" + String.Format("{0:00}", TimeLeft.Minutes) + ":"
                    + String.Format("{0:00}", TimeLeft.Seconds);
                else
                    buffer = TimeLeft.Hours.ToString() + " " + String.Format("{0:00}", TimeLeft.Minutes) + " "
                    + String.Format("{0:00}", TimeLeft.Seconds);
                return buffer;
            }
            public string ToString(bool token)
            {
                if (buffer.Contains(':'))
                    return buffer;
                else
                    return this.ToString();
            }
        }
        public class AnimationTask
        {
            public enum AnimationStage
            {
                First,
                Second,
                Third,
                Fourth,
                Completed
            }
            public AnimationTask(Move move)
            {
                Move = move;
                PieceMoving = move.PieceMoving;
                Square square = move.OriginalSquare;
                int midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                int midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                int dX = midX - PieceMoving.Image.Width / 2;
                int dY = midY - PieceMoving.Image.Height / 2;
                StartingPoint = new Point(dX, dY);

                square = move.DestSquare;
                midX = square.Rectangle.Location.X + square.Rectangle.Width / 2;
                midY = square.Rectangle.Location.Y + square.Rectangle.Height / 2;
                dX = midX - PieceMoving.Image.Width / 2;
                dY = midY - PieceMoving.Image.Height / 2;
                StoppingPoint = new Point(dX, dY);

                int incX = Math.Abs((StartingPoint.X - StoppingPoint.X) / 4);
                int incY = Math.Abs((StartingPoint.Y - StoppingPoint.Y) / 4);
                MidPoints = new List<Point>();
                for (int i = 1; i < 4; i++)
                {
                    Point point = new Point();
                    point.X = (StartingPoint.X < StoppingPoint.X ? 
                        StartingPoint.X + (incX * i) : StartingPoint.X - (incX * i));
                    point.Y = (StartingPoint.Y < StoppingPoint.Y ?
                        StartingPoint.Y + (incY * i) : StartingPoint.Y - (incY * i));
                    MidPoints.Add(point);
                }
                Stage = AnimationStage.First;
            }
            public Point StartingPoint { get; set; }
            public Move Move { get; set; }
            public Point StoppingPoint { get; set; }
            public List<Point> MidPoints { get; set; }
            public AnimationStage Stage { get; set; }
        }
        [Serializable]
        public class Arrow : IEquatable<Arrow>
        {
            public Arrow()
            {
                Pen = new Pen(BMAColor, 6F);
                this.Squares = new List<Square>();
                Enabled = false;
                System.Drawing.Drawing2D.AdjustableArrowCap aac =
                new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                aac.MiddleInset = 2;
                Pen.CustomEndCap = aac;
            }
            public Arrow(Color color)
            {
                Pen = new Pen(new SolidBrush(color), 8F);
                this.Squares = new List<Square>();
                Enabled = false;
                System.Drawing.Drawing2D.AdjustableArrowCap aac =
                new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                aac.MiddleInset = 2;
                Pen.CustomEndCap = aac;
            }
            public bool isShowing { get; set; }
            public Color Color { get; set; }
            public bool IsInvalid { get; set; }
            public bool Enabled { get; set; }
            public Square StartingSquare { get; set; }
            public Square StoppingSquare { get; set; }
            [NonSerialized]
            Pen pen;
            public Pen Pen { get { return pen; }
                set 
                {
                    pen = value;
                    if (pen != null)
                        Color = pen.Color;
                }
            }
            public override bool Equals(object obj)
            {
                if (obj is Arrow)
                    return this.Equals(obj as Arrow);
                else return false;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override string ToString()
            {
                return StartingSquare.Name + " to " + StoppingSquare.Name;
            }
            public bool Equals(Arrow other)
            {
                return (StartingSquare == other.StartingSquare &&
                    StoppingSquare == other.StoppingSquare);
            }
            public void Update()
            {
                if (StartingSquare == StoppingSquare)
                {
                    Squares = new List<Square>();
                    Squares.Add(StartingSquare);
                    return;
                }
                Point StartingPoint, StoppingPoint;
                int midX = StartingSquare.Rectangle.Location.X +
                    StartingSquare.Rectangle.Width / 2;
                int midY = StartingSquare.Rectangle.Location.Y +
                    StartingSquare.Rectangle.Height / 2;
                StartingPoint = new Point(midX, midY);

                midX = StoppingSquare.Rectangle.Location.X +
                    StoppingSquare.Rectangle.Width / 2;
                midY = StoppingSquare.Rectangle.Location.Y +
                    StoppingSquare.Rectangle.Height / 2;
                StoppingPoint = new Point(midX, midY);

                Point inc1, inc2, dec1, dec2;
                Size S = new Size(1, 1);
                if ((StartingPoint.Y - StoppingPoint.Y) ==
                    (StartingPoint.X - StoppingPoint.X))
                {
                    inc1 = new Point(StartingPoint.X + 1, StartingPoint.Y - 1);
                    dec1 = new Point(StartingPoint.X - 1, StartingPoint.Y + 1);
                    inc2 = new Point(StoppingPoint.X + 1, StoppingPoint.Y - 1);
                    dec2 = new Point(StoppingPoint.X - 1, StoppingPoint.Y + 1);
                }
                else
                {
                    inc1 = StartingPoint + S;
                    dec1 = StartingPoint - S;
                    inc2 = StoppingPoint + S;
                    dec2 = StoppingPoint - S; 
                }
                Point[] points = { inc1, dec1, dec2, inc2 };
                GraphicsPath gp = new GraphicsPath();
                gp.AddPolygon(points);
                Region region = new Region(gp);
                this.Squares = new List<Square>();
                foreach (var item in Form1.Squares)
                    if (region.IsVisible(item.Rectangle))
                        this.Squares.Add(item);
            }
            public List<Square> Squares { get; set; }
        }
        public interface IPlayer
        {
            Piece.PieceSide Side { get; set; }
            Clock Time { get; set; }
            IPlayer Opponent { get; set; }
            int Rating { get; set; }
            String Name { get; set; }
            bool isTempRating { get; set; }
            String ToString();
        }
        [Serializable]
        public class User : IPlayer
        {
            public User(String name)
            {
                Name = name;
                Rating = 1500;
                isTempRating = true;
                ListOfGames = new List<Tuple<string, int, GameDetails.Outcome, OpeningNode>>();
            }
            public User(String name, int rating)
            {
                Name = name;
                Rating = rating;
                isTempRating = false;
                ListOfGames = new List<Tuple<string, int, GameDetails.Outcome, OpeningNode>>();
            }
            public void ComputeRating()
            {
                ratingF = (PRTotalOppRating + (500 * PRWinLossDiff)) / PRGameCount;
                Rating = (int)Math.Round(ratingF, 0);
            }
            public decimal PRWinLossDiff { get; set; }
            public decimal PRGameCount { get; set; }
            public long PRTotalOppRating { get; set; }
            public Piece.PieceSide Side { get; set; }
            public List<Tuple<String, int, GameDetails.Outcome, OpeningNode>> ListOfGames { get; set; }
            decimal ratingF;
            public int Rating { get; set; }
            Clock time;
            public Clock Time { get { return time; } set { time = value; } }
            public String Name { get; set; }
            public bool isTempRating { get; set; }
            public override string ToString()
            {
                return Name;
            }
            IPlayer opponent;
            public IPlayer Opponent { get { return opponent; } set { opponent = value; } }
        }
        public class PresetTheme
        {
            [Serializable]
            public enum Theme
            {
                Coffee,
                Metro,
                Linen,
                Brick,
                Silver,
                Wood
            }
            public PresetTheme(Theme theme)
            {
                ColorTheme = theme;
                switch (theme)
                {
                    case Theme.Coffee:
                        this.Name = "Coffee";
                        this.LightSquareColor = new SolidBrush(Color.FromArgb(255, 220, 183, 90));
                        this.DarkSquareColor = new SolidBrush(Color.FromArgb(255, 150, 32, 20));
                        this.LegalMHLColor = Color.FromArgb(150, 75, 120, 53);
                        this.LastMoveColor = Color.FromArgb(150, 75, 120, 53);
                        this.ShiftColor = Color.FromArgb(150, 75, 120, 53);
                        this.CtrlColor = Color.FromArgb(150, Color.OrangeRed);
                        this.AltColor = Color.FromArgb(150, Color.CadetBlue);
                        this.BMAColor = Color.FromArgb(220, Color.Red);
                        break;

                    case Theme.Silver:
                        this.Name = "Silver";
                        this.LightSquareColor = Brushes.OldLace;
                        this.DarkSquareColor = Brushes.DarkTurquoise;
                        this.LegalMHLColor = Color.FromArgb(150, Color.YellowGreen);
                        this.LastMoveColor = Color.FromArgb(150, Color.YellowGreen);
                        this.ShiftColor = Color.FromArgb(150, 75, 120, 53);
                        this.AltColor = Color.FromArgb(150, Color.CadetBlue);
                        this.CtrlColor = Color.FromArgb(150, Color.DarkOrange);
                        this.BMAColor = Color.FromArgb(220, Color.Red);
                        break;

                    case Theme.Linen:
                        this.Name = "Linen";
                        Bitmap bmp = new Bitmap(Properties.Resources.DarkLinen);
                        using (Graphics g = Graphics.FromImage(bmp))
                            g.FillRectangle(new SolidBrush(Color.FromArgb(10, Color.Black)), 
                                0, 0, bmp.Width, bmp.Height);
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        this.LightSquareColor = new TextureBrush(new Bitmap(Properties.Resources.LightLinen),
                            WrapMode.TileFlipXY, new Rectangle(new Point(5, 5), new Size(40, 40)));
                        this.DarkSquareColor = new TextureBrush(bmp,
                            WrapMode.TileFlipXY, new Rectangle(new Point(5, 5), new Size(40, 40)));
                        this.LegalMHLColor = Color.FromArgb(150, 255, 0, 0);   // OrangeRed Dark
                        this.LastMoveColor = Color.FromArgb(150, 255, 90, 90);      // OrangeRed Light
                        this.ShiftColor = Color.FromArgb(150, 75, 120, 53);
                        this.AltColor = Color.FromArgb(150, Color.DarkOrange);
                        this.CtrlColor = Color.FromArgb(150, Color.CornflowerBlue);
                        this.BMAColor = Color.FromArgb(220, Color.Red);
                        break;
                    case Theme.Wood:
                        this.Name = "Wood";
                        Bitmap bm = new Bitmap(Properties.Resources.DarkWood);
                        using (Graphics g = Graphics.FromImage(bm))
                            g.FillRectangle(new SolidBrush(Color.FromArgb(10, Color.Black)), 
                                0, 0, bm.Width, bm.Height);
                        bm.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        this.LightSquareColor = new TextureBrush(new Bitmap(Properties.Resources.LightWood),
                            WrapMode.TileFlipXY, new Rectangle(new Point(5, 5), new Size(40, 40)));
                        this.DarkSquareColor = new TextureBrush(bm,
                            WrapMode.TileFlipXY, new Rectangle(new Point(5, 5), new Size(40, 40)));
                        this.LegalMHLColor = Color.FromArgb(150, Color.DarkOliveGreen);
                        this.LastMoveColor = Color.FromArgb(150, Color.DarkOliveGreen);
                        this.ShiftColor = Color.FromArgb(150, 75, 120, 53);
                        this.AltColor = Color.FromArgb(150, Color.Yellow);
                        this.CtrlColor = Color.FromArgb(150, Color.CornflowerBlue);
                        this.BMAColor = Color.FromArgb(220, Color.Red);
                        break;

                    case Theme.Brick:
                        this.Name = "Brick";
                        this.LightSquareColor = new TextureBrush(new Bitmap(Properties.Resources.LightWood));
                        this.DarkSquareColor = new TextureBrush(new Bitmap(Properties.Resources.DarkWood));
                        this.LegalMHLColor = Color.FromArgb(150, Color.YellowGreen);
                        this.LastMoveColor = Color.FromArgb(150, Color.YellowGreen);
                        this.ShiftColor = Color.FromArgb(150, 75, 120, 53);
                        this.AltColor = Color.FromArgb(150, Color.CadetBlue);
                        this.CtrlColor = Color.FromArgb(150, Color.DarkOrange);
                        this.BMAColor = Color.FromArgb(220, Color.Red);
                        break;
                    case Theme.Metro:
                        this.Name = "Metro";
                        this.LightSquareColor = Brushes.Azure;
                        this.DarkSquareColor = new SolidBrush(Color.FromArgb(255, 178, 64, 64));  //Brushes.Firebrick;
                        this.LegalMHLColor = Color.FromArgb(150, SystemColors.ControlDarkDark);
                        this.LastMoveColor = Color.FromArgb(150, SystemColors.ControlDarkDark);
                        this.ShiftColor = Color.FromArgb(150, 75, 120, 53);
                        this.AltColor = Color.FromArgb(150, Color.CadetBlue);
                        this.CtrlColor = Color.FromArgb(150, Color.Yellow);
                        this.BMAColor = Color.FromArgb(220, Color.Red);
                        break;
                }
                this.UserArrowColors = new List<Color>();
                this.UserArrowColors.Add(this.ShiftColor);
                this.UserArrowColors.Add(this.CtrlColor);
                this.UserArrowColors.Add(this.AltColor);    
            }
            public String Name { get; set; }
            public Brush LightSquareColor { get; set; }
            public Brush DarkSquareColor { get; set; }
            public Color ShiftColor { get; set; }
            public Color CtrlColor { get; set; }
            public Theme ColorTheme { get; set; }
            public Color AltColor { get; set; }
            public List<Color> UserArrowColors { get; set; }
            public Color LastMoveColor { get; set; }
            public Color LegalMHLColor { get; set; }
            public Color KingInCheckColor { get; set; }
            public Color BMAColor { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
