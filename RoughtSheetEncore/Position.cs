using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RoughtSheetEncore
{
    public partial class Form1 : Form
    {

        #region Declarations
        

        List<Label> VariationLabels;
        Form GameDetailForm, CommentForm;
        Size ScrollSize1, ScrollSize2;
        int focus = 0, index;
        bool quickLoad = false, ShouldAnimate, ShouldHighlightLastMove, IsAnimating;
        Label focusLabel, LabelClicked;
        TextBox WhiteNameBox, WhiteEloBox, BlackNameBox, BlackEloBox, EventBox, SiteBox,
            RoundBox, ECOBox, DateBox;
        RadioButton WhiteWinButton, BlackWinButton, DrawButton;
        RadioButton NoMoveComment, StrongMove, WeakMove, InterestingMove, DubiousMove,
            GreatMove, TerribleMove, NoPositionComment, EqualPosition, SlightWhiteAdv,
            SlightBlackAdv, ModerateWhiteAdv, ModerateBlackAdv, DecisiveWhiteAdv, DecisiveBlackAdv;
        TextBox LongCommentBox;
        Font NotationFont = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Regular);
        Color NotationMainForeColor = Color.Black;
        Color NotationFocusColor = Color.ForestGreen;
        Color NotationSubForeColor = SystemColors.ControlDark;
        ContextMenuStrip NotationContextMenu;
        ToolStripMenuItem CopyGameMenuItem = new ToolStripMenuItem("Copy Game to ClipBoard");
        ToolStripMenuItem PasteGameMenuItem = new ToolStripMenuItem("Paste Game from ClipBoard");
        ToolStripMenuItem GameDetailsMenuItem = new ToolStripMenuItem("Game Details");
        ToolStripMenuItem EnterCommentMenuItem = new ToolStripMenuItem("Enter Comment");
        ToolStripMenuItem DeleteVariationMenuItem = new ToolStripMenuItem("Delete Variation");
        ToolStripMenuItem MoveVariationUpMenuItem = new ToolStripMenuItem("Move Variation Up");
        ToolStripMenuItem DeleteRestOfGameMenuItem = new ToolStripMenuItem("Delete Rest of Game");
        ToolStripMenuItem FontNotationMenuItem = new ToolStripMenuItem("Font...");
        ToolStripMenuItem DeleteAllVariationsMenuItem = new ToolStripMenuItem("Delete All Variations");
        ToolStripMenuItem DeleteAllAnotationsMenuItem = new ToolStripMenuItem("Delete All Annotations");
        List<OpeningNode> TabierList;
        OpeningNode ActiveTabier;
        Form ECOForm;
        Label SSLastNumLabel, SSCurrentLabel;
#endregion

        private void SavePosition(Move move, bool isPlayOut)
        {
            move.MoveNo = (MoveCount + 1) / 2;
            Position TempPosition = new Position();
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
            TempPosition.LastMovePlayed = move;
            TempPosition.PieceInfos = ClonePieces();
            TempPosition.QueensideCastlingBlack = QueensideCastlingBlack;
            TempPosition.QueensideCastlingWhite = QueensideCastlingWhite;
            TempPosition.sideToPlay = sideToPlay;
            TempPosition.MoveCount = MoveCount;
            TempPosition.FiftyMoveCount = FiftyMoveCount;
            CurrentVariation.MovesList.Add(TempPosition);
            CurrentPosition = TempPosition;

            if (ModeOfPlay != PlayMode.EditPosition && !isPlayOut && CheckFor3FRDraw() && 
                gameDetails.Result != GameDetails.Outcome.Draw && !IsDraw)
            {
                if (CurrentVariation == MainLine)
                {
                    gameDetails.Result = GameDetails.Outcome.Draw;
                    OnGameEnding();
                    ShowGameDetails();
                }
                ModeOfPlay = PlayMode.EditPosition;
                IsDraw = true;
                ShouldClockTick = false;
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
                MessageBox.Show("Draw by Three-fold Repitition");
            }

            if (!isPlayOut || (isEngineMatchInProgress && ModeOfPlay == PlayMode.EngineVsEngine))
            {
                if (!IsAnimating && LastMoveHighlighted != CurrentPosition.LastMovePlayed)
                    HighLightLastMove(CurrentPosition.LastMovePlayed);
                if (!IsAnimating)
                    HighlightCheckedKing();
                flowLayoutPanel1.SuspendLayout();
                if (newVarOption != NewVariationOption.NewMainLine && newVarOption != NewVariationOption.Overwrite)
                    InsertMoveNotation(move);
                flowLayoutPanel1.ResumeLayout();
                flowLayoutPanel1.ScrollControlIntoView(focusLabel);
                if (IsWhiteCheckmated || IsBlackCheckmated)
                {
                    ShouldClockTick = false;
                    OnGameEnding();
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
                    MessageBox.Show("Checkmate. " + (IsBlackCheckmated ?
                    "White wins" : "Black Wins"));
                }
            }
            if (!isPlayOut)
            {
                ShowOpening();
                //DisplayECOForm();
                if (ModeOfPlay != PlayMode.EditPosition)
                    TapClock();
            }
        }
        private void ShowOpening()
        {
            if (CurrentPosition.MoveCount >= 30)
            {
                if (gameDetails.FinalTabier == null)
                    return;
                ActiveTabier = gameDetails.FinalTabier;
                OpeningLabel.Text = gameDetails.FinalTabier.ECOCode + " - " + gameDetails.FinalTabier.Name;
                gameDetails.ECO = gameDetails.FinalTabier.ECOCode;
                OpeningLabel.Location = new Point(tabPage1.Width / 2 - OpeningLabel.Width / 2, flowLayoutPanel1.Top - 30);
                SSOpening.Text = gameDetails.FinalTabier.ECOCode + " - " + gameDetails.FinalTabier.Name;
                SSOpening.Location = new Point(tabPage2.Width / 2 - SSOpening.Width / 2, SSOpening.Location.Y);
                toolTip1.SetToolTip(OpeningLabel, OpeningLabel.Text);
                toolTip1.SetToolTip(SSOpening, SSOpening.Text);
                return;
            }
            ActiveTabier = null;
            if (TabierList != null && CurrentPosition != StartingPosition)
                foreach (var item in TabierList)
                {
                    if (CurrentPosition.Compare(item))
                    {
                        ActiveTabier = item;
                        if (CurrentPosition == CurrentVariation.MovesList[CurrentVariation.MovesList.Count - 1])
                            gameDetails.FinalTabier = item;
                        OpeningLabel.Text = item.ECOCode + " - " + item.Name;
                        gameDetails.ECO = item.ECOCode;
                        OpeningLabel.Location = new Point(tabPage1.Width / 2 - 
                            OpeningLabel.Width / 2, flowLayoutPanel1.Top - 30);
                        SSOpening.Text = item.ECOCode + " - " + item.Name;
                        SSOpening.Location = new Point(tabPage2.Width / 2 - SSOpening.Width / 2, SSOpening.Location.Y);
                        toolTip1.SetToolTip(OpeningLabel, OpeningLabel.Text);
                        toolTip1.SetToolTip(SSOpening, SSOpening.Text);
                        break;
                    }
                }
            else
            {
                ActiveTabier = null;
                OpeningLabel.Text = "";
                SSOpening.Text = "";
            }

            if (ActiveTabier == null && CurrentPosition != StartingPosition && gameDetails.FinalTabier != null)
            {
                ActiveTabier = gameDetails.FinalTabier;
                OpeningLabel.Text = gameDetails.FinalTabier.ECOCode + " - " + gameDetails.FinalTabier.Name;
                gameDetails.ECO = gameDetails.FinalTabier.ECOCode;
                OpeningLabel.Location = new Point(tabPage1.Width / 2 - OpeningLabel.Width / 2, flowLayoutPanel1.Top - 30);
                SSOpening.Text = gameDetails.FinalTabier.ECOCode + " - " + gameDetails.FinalTabier.Name;
                SSOpening.Location = new Point(tabPage2.Width / 2 - SSOpening.Width / 2, SSOpening.Location.Y);
                toolTip1.SetToolTip(OpeningLabel, OpeningLabel.Text);
                toolTip1.SetToolTip(SSOpening, SSOpening.Text);
            }
        }
        private bool CheckFor3FRDraw()
        {
            VariationHolder CurrentNode = CurrentVariation;
            int RepCount = 0;
            while (true)
            {
                for (int i = CurrentNode.MovesList.Count - 1; i >= 0; i--)
                {
                    if (CurrentNode != CurrentVariation && i > CurrentVariation.ParentIndex)
                        continue;

                    if (CurrentNode.MovesList[i] == CurrentPosition)
                        continue;
                    if (CurrentNode.MovesList[i] != MainLine.MovesList[0] && 
                        (CurrentNode.MovesList[i].LastMovePlayed.IsCapture ||
                        CurrentNode.MovesList[i].LastMovePlayed.PieceMoving.Type
                        == Piece.PieceType.Pawn))
                        return false;

                    else if (CurrentPosition.Compare(CurrentNode.MovesList[i]))
                    {
                        RepCount++;
                        if (RepCount == 2)
                            return true;
                    }
                }
                if (CurrentNode.ParentLine != null)
                    CurrentNode = CurrentNode.ParentLine;
                else
                    break;
            }
            return false;
        }
        private void DisplayECOForm()
        {
            if (CurrentPosition == StartingPosition)
            {
                return;
            }
            if (ECOForm == null)
            {
                ECOForm = new Form();
                ECOForm.StartPosition = FormStartPosition.CenterParent;
                TextBox tb1 = new TextBox();
                TextBox tb2 = new TextBox();
                tb1.Size = new System.Drawing.Size(150, 20);
                tb2.Size = new System.Drawing.Size(150, 40);
                tb1.Font = new System.Drawing.Font("Calibri", 10F, FontStyle.Regular);
                tb2.Font = new System.Drawing.Font("Calibri", 10F, FontStyle.Regular);
                tb2.Multiline = true;
                Label l1 = new Label();
                Label l2 = new Label();
                l1.Text = "ECO Code";
                l2.Text = "Name";
                l1.Location = new Point(20, 20);
                l2.Location = new Point(20, 60);
                tb1.Location = new Point(90, 20);
                tb2.Location = new Point(90, 60);
                ECOForm.Controls.Add(tb1);
                ECOForm.Controls.Add(tb2);
                ECOForm.Controls.Add(l1);
                ECOForm.Controls.Add(l2);
                ECOForm.KeyDown += ECOForm_KeyDown;
                tb1.KeyDown += ECOForm_KeyDown;
                tb2.KeyDown += ECOForm_KeyDown;
            }
            OpeningNode temp = null;
            if (TabierList != null)
            foreach (var item in TabierList)
            {
                if (CurrentPosition.Compare(item))
                {
                    temp = item;
                    ECOForm.Controls[0].Text = item.ECOCode;
                    ECOForm.Controls[1].Text = item.Name;
                    break;
                }
            }
            if (temp == null)
            {
                ECOForm.Controls[0].Text = "";
                ECOForm.Controls[1].Text = "";
            }
            ECOForm.ShowDialog(this);

            OpeningNode opn = (temp == null ? new OpeningNode() : temp);
            opn.Name = ECOForm.Controls[1].Text;
            opn.ECOCode = ECOForm.Controls[0].Text;
            if (temp == null)
            {
                if (TabierList == null)
                    TabierList = new List<OpeningNode>();
                opn.PieceInfos = CurrentPosition.PieceInfos;
                opn.CreateStringTuple();
                String str = "";
                foreach (var item in MainLine.MovesList)
                {
                    if (item != StartingPosition)
                    {
                        str += item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White ?
                            item.LastMovePlayed.MoveNo + ". " : "";
                        str += item.LastMovePlayed.ShortNotation +" ";
                    }
                }
                opn.OpeningLine = str;
                TabierList.Add(opn);
            }
            if (ECOForm.Controls[1].Text == "" && ECOForm.Controls[0].Text == "")
            {
                TabierList.Remove(opn);
            }


            if (TabierList == null)
                return;
            BinaryFormatter bf = new BinaryFormatter();
            using (Stream output = File.Create
                (Environment.SpecialFolder.MyDocuments + @"\Chess Database\Grandmaster\\zarion.dat"))
            {
                bf.Serialize(output, TabierList);
            }
            if (TabierList.Count % 10 == 0 && TabierList.Count > 100)
            {
                using (Stream output = File.Create
                (Environment.SpecialFolder.MyDocuments + @"\Chess Database\Grandmaster\\zarion2.dat"))
                {
                    bf.Serialize(output, TabierList);
                }
            }
        }
        void ECOForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                ECOForm.Close();
            }
        }
        private void InsertMoveNotation(Move move)
        {
            UpdateScoreSheet(move);
            flowLayoutPanel1.Size = ScrollSize1;
            Label tempLabel = new Label();
            tempLabel.Name = (move.PieceMoving.Side == Piece.PieceSide.White ? 'W' : 'B') +move.MoveNo.ToString();
            tempLabel.Tag = CurrentVariation;
            if (move.PieceMoving.Side == Piece.PieceSide.White)
                tempLabel.Text += move.MoveNo.ToString() + ". ";
            else if (flowLayoutPanel1.Controls.Count == 0)
                tempLabel.Text += move.MoveNo.ToString() + ". . .";
            tempLabel.Text += move.ShortNotation;
            tempLabel.Font = NotationFont;
            tempLabel.Margin = new System.Windows.Forms.Padding(0);
            tempLabel.ContextMenuStrip = NotationContextMenu;
            tempLabel.MouseDown += NotationLabel_MouseDown;
            tempLabel.AutoSize = true;
            tempLabel.ForeColor = NotationFocusColor;
            tempLabel.BackColor = flowLayoutPanel1.BackColor;
            if (focusLabel != null)
            {
                if (focusLabel.Tag == MainLine)
                    focusLabel.ForeColor = NotationMainForeColor;
                else
                    focusLabel.ForeColor = NotationSubForeColor;
            }
                           
            if (CurrentVariation != MainLine && CurrentPosition == CurrentVariation.MovesList[0])
            {
                Label braceLabel1 = new Label(), braceLabel2 = new Label();
                braceLabel1.Tag = CurrentVariation;
                braceLabel2.Tag = CurrentVariation;
                braceLabel1.Name = "X";
                braceLabel2.Name = "Y";
                if (CurrentVariation.ParentLine == MainLine)
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
                Label indexLabel = new Label();
                foreach (var item in flowLayoutPanel1.Controls)
                {
                    indexLabel = item as Label;
                    if (indexLabel.Name == tempLabel.Name && indexLabel.Tag == CurrentVariation.ParentLine)
                        break;
                    else
                        indexLabel = null;
                }
                if (indexLabel != null)
                {
                    flowLayoutPanel1.Controls.SetChildIndex(braceLabel1, flowLayoutPanel1.Controls.IndexOf(indexLabel) + 1);
                    if (CurrentVariation.ParentLine == MainLine)
                    {
                        flowLayoutPanel1.SetFlowBreak(indexLabel, true);
                    }
                        
                }
                else
                    MessageBox.Show("ERROR! \nCode: 073840");
                flowLayoutPanel1.Controls.SetChildIndex(braceLabel2, flowLayoutPanel1.Controls.IndexOf(braceLabel1) + 1);
                if (CurrentVariation.ParentLine == MainLine)
                {
                    flowLayoutPanel1.SetFlowBreak(braceLabel2, true);
                }

                tempLabel.AccessibleName = "break";

                if (move.PieceMoving.Side == Piece.PieceSide.White)
                    tempLabel.Text = move.MoveNo.ToString() + ". ";
                else
                    tempLabel.Text = move.MoveNo.ToString() + " . . . ";
                tempLabel.Text += move.ShortNotation;
                flowLayoutPanel1.Controls.Add(tempLabel);
                flowLayoutPanel1.Controls.SetChildIndex(tempLabel, flowLayoutPanel1.Controls.IndexOf(braceLabel1) + 1);
                focusLabel = tempLabel;
                flowLayoutPanel1.Refresh();
            }
            else
            {
                flowLayoutPanel1.Controls.Add(tempLabel);

                //if (newVarOption == NewVariationOption.NewMainLine)
                //{
                //    flowLayoutPanel1.Controls.SetChildIndex(tempLabel, flowLayoutPanel1.Controls.IndexOf(focusLabel) + 1);
                //    if (CurrentVariation == MainLine)
                //    {
                //        flowLayoutPanel1.SetFlowBreak(tempLabel, true);
                //    }
                //}
                if (focusLabel != null)
                {
                    Label label = (Label)flowLayoutPanel1.Controls[flowLayoutPanel1.Controls.IndexOf(focusLabel) + 1];
                    if (label.Name == "X")
                    {
                        int i = 0;
                        foreach (var itemx in flowLayoutPanel1.Controls)
                        {
                            Label item = itemx as Label;
                            if (item.Tag == label.Tag && item.Name == "Y")
                            {
                                flowLayoutPanel1.Controls.SetChildIndex(tempLabel, i + 1);
                            }
                            i++;
                        }
                    }

                    else if (focusLabel != null || CurrentVariation != MainLine)
                        flowLayoutPanel1.Controls.SetChildIndex(tempLabel, flowLayoutPanel1.Controls.IndexOf(focusLabel) + 1);
                }

                focusLabel = tempLabel;
            }
            if (flowLayoutPanel1.VerticalScroll.Visible)
                flowLayoutPanel1.Size = ScrollSize2;
        }
        private void UpdateScoreSheet(Move move)
        {
            if (CurrentVariation != MainLine)
            {
                SSCurrentLabel = null;
                List<Label> LabeList = SSPanel.Tag as List<Label>;
                foreach (var item in LabeList)
                {
                    item.ForeColor = Color.DimGray;
                }
                return;
            }

            Label numLabel = null, notationLabel = new Label();
            if (SSLastNumLabel != null)
            {
                if (SSLastNumLabel.Tag is Label)
                    ((Label)SSLastNumLabel.Tag).ForeColor = Color.Black;
                else
                {
                    ((Tuple<Label, Label>)SSLastNumLabel.Tag).Item2.ForeColor = Color.Black;
                }
            }

            if (move.PieceMoving.Side == Piece.PieceSide.White)
            {
                numLabel = new Label();
                numLabel.Tag = notationLabel;
                numLabel.Text = move.MoveNo.ToString();
                if (SSLastNumLabel == null)
                {
                    numLabel.Location = new Point(10, 1);
                }
                else
                {
                    numLabel.Location = SSLastNumLabel.Location + new Size(0, 20);
                }
                SSLastNumLabel = numLabel;
                numLabel.Font = new System.Drawing.Font("Calibri", 11F, FontStyle.Bold);
                notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                notationLabel.Location = numLabel.Location + new Size(30, -2);
                notationLabel.Text = move.ShortNotation;
                notationLabel.Tag = Tuple.Create(CurrentPosition, SSLastNumLabel);
                numLabel.AutoSize = true;
                notationLabel.AutoSize = true;
                SSPanel.Controls.Add(numLabel);
                SSPanel.Controls.Add(notationLabel);
            }
            else
            {
                if (SSLastNumLabel != null)
                {
                    SSLastNumLabel.Tag = Tuple.Create(SSLastNumLabel.Tag as Label, notationLabel);
                    notationLabel.AutoSize = true;
                    SSPanel.Controls.Add(notationLabel);
                    notationLabel.Text = move.ShortNotation;
                    notationLabel.Tag = Tuple.Create(CurrentPosition, SSLastNumLabel);
                    notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                    notationLabel.Location = SSLastNumLabel.Location + new Size(100, -2); 
                }
                else
                {
                    numLabel = new Label();
                    numLabel.Tag = notationLabel;
                    numLabel.Text = move.MoveNo.ToString();
                    numLabel.Location = new Point(10, 1);
                    SSLastNumLabel = numLabel;
                    numLabel.Font = new System.Drawing.Font("Calibri", 11F, FontStyle.Bold);
                    notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                    notationLabel.Location = numLabel.Location + new Size(30, -2);
                    notationLabel.Text = " ";
                    notationLabel.Tag = Tuple.Create(CurrentPosition, SSLastNumLabel);
                    numLabel.AutoSize = true;
                    notationLabel.AutoSize = true;
                    SSPanel.Controls.Add(numLabel);
                    SSPanel.Controls.Add(notationLabel);

                    SSLastNumLabel.Tag = Tuple.Create(SSLastNumLabel.Tag as Label, notationLabel);
                    notationLabel.AutoSize = true;
                    SSPanel.Controls.Add(notationLabel);
                    notationLabel.Text = move.ShortNotation;
                    notationLabel.Tag = Tuple.Create(CurrentPosition, SSLastNumLabel);
                    notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                    notationLabel.Location = SSLastNumLabel.Location + new Size(100, -2); 
                }
            }
            notationLabel.MouseDown += SSLabel_Click;
            SSCurrentLabel = notationLabel;
            SSPanel.ScrollControlIntoView(SSPanel.Controls[SSPanel.Controls.Count - 1]);
            List<Label> list = SSPanel.Tag == null ? new List<Label>() : SSPanel.Tag as List<Label>;
            list.Add(notationLabel);
            SSPanel.Tag = list;
        }
        void SSLabel_Click(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            SSCurrentLabel = label;
            CurrentVariation = MainLine;
            LoadPosition(((Tuple<Position, Label>)label.Tag).Item1, false);
            UpdateFocusLabel();
        }
        void flowLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            DeleteVariationMenuItem.Enabled = false;
            MoveVariationUpMenuItem.Enabled = false;
            DeleteRestOfGameMenuItem.Enabled = false;
            EnterCommentMenuItem.Enabled = false;

            try
            {
                if (Clipboard.ContainsText())
                    PasteGameMenuItem.Enabled = true;
                else
                    PasteGameMenuItem.Enabled = false;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                
            }
        }
        void NotationLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Label)
            {
                Label tempLabel = sender as Label;
                VariationHolder tempVar = tempLabel.Tag as VariationHolder;
                foreach (var item in tempVar.MovesList)
                {
                    if (item.LastMovePlayed != null)
                        if (tempLabel.Name.Substring(1, tempLabel.Name.Length - 1) == item.LastMovePlayed.MoveNo.ToString())
                        {
                            if ((tempLabel.Name[0] == 'B' && item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.Black) ||
                                (tempLabel.Name[0] == 'W' && item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White))
                            {
                                CurrentVariation = tempVar;
                                LoadPosition(item, false);
                                if (focusLabel != null)
                                {
                                    focusLabel.Font = NotationFont;
                                    if (focusLabel.Tag == MainLine)
                                        focusLabel.ForeColor = NotationMainForeColor;
                                    else
                                        focusLabel.ForeColor = NotationSubForeColor;
                                }
                                focusLabel = tempLabel;
                                tempLabel.ForeColor = NotationFocusColor;
                                tempLabel.Font = NotationFont;
                                flowLayoutPanel1.Refresh();
                                break;
                            }
                        }
                }
            }
        }
        private void InitializeNotationContextMenu()
        {
            NotationContextMenu = new ContextMenuStrip();
            NotationContextMenu.Items.Add(CopyGameMenuItem);
            NotationContextMenu.Items.Add(PasteGameMenuItem);
            NotationContextMenu.Items.Add(GameDetailsMenuItem);
            NotationContextMenu.Items.Add(EnterCommentMenuItem);
            NotationContextMenu.Items.Add(new ToolStripSeparator());
            NotationContextMenu.Items.Add(MoveVariationUpMenuItem);
            NotationContextMenu.Items.Add(DeleteVariationMenuItem);
            NotationContextMenu.Items.Add(DeleteRestOfGameMenuItem);
            NotationContextMenu.Items.Add(DeleteAllVariationsMenuItem);
            NotationContextMenu.Items.Add(DeleteAllAnotationsMenuItem);
            NotationContextMenu.Items.Add(new ToolStripSeparator());
            NotationContextMenu.Items.Add(FontNotationMenuItem);
            CopyGameMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            PasteGameMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            EnterCommentMenuItem.ShortcutKeys = Keys.Control | Keys.M;
            CopyGameMenuItem.Click += NotationMenu_Click;
            PasteGameMenuItem.Click += NotationMenu_Click;
            DeleteAllAnotationsMenuItem.Click += NotationMenu_Click;
            DeleteAllVariationsMenuItem.Click += NotationMenu_Click;
            GameDetailsMenuItem.Click += NotationMenu_Click;
            EnterCommentMenuItem.Click += NotationMenu_Click;
            DeleteVariationMenuItem.Click += NotationMenu_Click;
            MoveVariationUpMenuItem.Click += NotationMenu_Click;
            DeleteRestOfGameMenuItem.Click += NotationMenu_Click;
            FontNotationMenuItem.Click += NotationMenu_Click;

            flowLayoutPanel1.ContextMenuStrip = NotationContextMenu;
            tabPage1.ContextMenuStrip = NotationContextMenu;
            tabPage2.ContextMenuStrip = NotationContextMenu;
            tabControl1.ContextMenuStrip = NotationContextMenu;
            SSPanel.ContextMenuStrip = NotationContextMenu;
            NotationContextMenu.Opening += NotationContextMenu_Opening;
        }
        void NotationContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CurrentPosition.LastMovePlayed != null)
            {
                DeleteRestOfGameMenuItem.Enabled = true;
                EnterCommentMenuItem.Enabled = true;
            }
            else
            {
                DeleteRestOfGameMenuItem.Enabled = false;
                EnterCommentMenuItem.Enabled = false;
            }

            try
            {
                if (Clipboard.ContainsText())
                    PasteGameMenuItem.Enabled = true;
                else
                    PasteGameMenuItem.Enabled = false;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                PasteGameMenuItem.Enabled = false;
            }

            LabelClicked = sender as Label;
            if (CurrentVariation == MainLine)
            {
                DeleteVariationMenuItem.Enabled = false;
                MoveVariationUpMenuItem.Enabled = false;
            }
            else
            {
                DeleteVariationMenuItem.Enabled = true;
                MoveVariationUpMenuItem.Enabled = true;
            }
        }
        void NotationMenu_Click(object sender, EventArgs e)
        {
            if (sender == FontNotationMenuItem)
            {
                FontDialog fontDialog = new FontDialog();
                fontDialog.Font = NotationFont;
                fontDialog.ShowDialog(this);
                NotationFont = fontDialog.Font;
                flowLayoutPanel1.SuspendLayout();
                foreach (var itemx in flowLayoutPanel1.Controls)
                {
                    Label item = itemx as Label;
                    item.Font = NotationFont;
                }
                flowLayoutPanel1.ResumeLayout();
            }
            else if (sender == DeleteVariationMenuItem)
                DeleteVariation(LabelClicked != null ? (VariationHolder)LabelClicked.Tag :
                    (VariationHolder)focusLabel.Tag);
            else if (sender == MoveVariationUpMenuItem)
                MoveVariationUp(LabelClicked);
            else if (sender == CopyGameMenuItem)
                CopyGame();
            else if (sender == DeleteAllVariationsMenuItem)
                DeleteAllVariations();
            else if (sender == DeleteAllAnotationsMenuItem)
                DeleteAllAnotations();
            else if (sender == GameDetailsMenuItem)
                DisplayGameDetails();
            else if (sender == DeleteRestOfGameMenuItem)
                DeleteRestOfGame();
            else if (sender == EnterCommentMenuItem)
                DisplayCommentForm();
            else if (sender == PasteGameMenuItem)
                PasteGameFromClipBoard();
        }
        private void DeleteAllAnotations()
        {
            foreach (VariationHolder item in GameVariations)
                foreach (var position in item.MovesList)
                {
                    if (position.LastMovePlayed == null)
                        continue;
                    position.LastMovePlayed.Comment = new Comment();
                }
            ReInsertLabels();
        }
        private void DeleteAllVariations()
        {
            if (CurrentVariation != MainLine)
            {
                VariationHolder vh = CurrentVariation;
                while (true)
                {
                    if (vh.ParentLine != MainLine)
                        vh = vh.ParentLine;
                    else
                    {
                        LoadPosition(MainLine.MovesList[vh.ParentIndex], false);
                        break;
                    }
                }
            }

            List<VariationHolder> list = new List<VariationHolder>();
            foreach (var item in GameVariations)
                if (item != MainLine)
                    list.Add(item);
            foreach (var item in list)
                GameVariations.Remove(item);
            foreach (var item in MainLine.MovesList)
                item.VariationHolders = null;

            ReInsertLabels();
        }
        private void DeleteRestOfGame()
        {
            int x = CurrentVariation.MovesList.IndexOf(CurrentPosition);
            if (x == CurrentVariation.MovesList.Count - 1)
                return;
            CurrentVariation.MovesList.RemoveRange(x + 1, CurrentVariation.MovesList.Count - x - 1);
            ReInsertLabels();
        }
        private void PasteGameFromClipBoard()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    ClipBoardBuffer = Clipboard.GetText();
                    VariationHolder ml = MainLine;
                    List<VariationHolder> gv = GameVariations;
                    Position cp = CurrentPosition;
                    VariationHolder cv = CurrentVariation;
                    StartNewGame(true);
                    Thread t = new Thread(LoadPgnGame);
                    t.Start();
                    panel1.SuspendLayout();
                    foreach (var item in Squares)
                    {
                        using (Graphics g = panel1.CreateGraphics())
                        {
                            if (item.Type == Square.SquareType.Dark)
                                g.FillRectangle((DarkSquareColor), item.Rectangle);
                            else
                                g.FillRectangle((LightSquareColor), item.Rectangle);
                        }
                    }
                    foreach (var item in StartingPosition.PieceInfos)
                    {
                        PlacePiece(item.Piece, item.Square, this);
                    }
                    t.Join();
                    if (!gameDetails.isUserGame)
                        ShowGameDetails();
                    else
                    {
                        LoadPosition(cp, false);
                        Graphics g = panel1.CreateGraphics();
                        foreach (var item in Squares)
                        {
                            g.FillRectangle(item.Type == Square.SquareType.Dark ?
                                DarkSquareColor : LightSquareColor, item.Rectangle);
                            if (item.Piece != null)
                                PlacePiece(item.Piece, item, this);
                        }
                        GameVariations = gv;
                        MainLine = ml;
                        CurrentVariation = cv;
                        ReInsertLabels();
                        if (focusLabel != null)
                        {
                            focusLabel.ForeColor = NotationFocusColor;
                            flowLayoutPanel1.ScrollControlIntoView(focusLabel); 
                        }
                        SSPanel.ScrollControlIntoView(SSCurrentLabel);
                        return;
                    }

                    bool STOP = false;
                    for (int i = 30; i > 0; i--)
                    {
                        if (STOP)
                            break;
                        if (i > MainLine.MovesList.Count - 1)
                            continue;
                        foreach (var item in TabierList)
                            if (MainLine.MovesList[i].Compare(item))
                            {
                                gameDetails.FinalTabier = item;
                                STOP = true;
                                break;
                            }
                    }

                    foreach (var item in LoadedEngines)
                    {
                        if (item.isUciEngine)
                        {
                            SendPositionToEngine(item, "");
                            ClearEngineOutput(item);
                            if (isInfiniteSearch)
                                item.Process.StandardInput.WriteLine("go infinite");
                        }
                    }
                    UpdateArrow(true);

                    ReInsertLabels();
                    if (flowLayoutPanel1.VerticalScroll.Visible && flowLayoutPanel1.Size != ScrollSize2)
                    {
                        flowLayoutPanel1.Size = ScrollSize2;
                    }
                    else if (!flowLayoutPanel1.VerticalScroll.Visible && flowLayoutPanel1.Size != ScrollSize1)
                    {
                        flowLayoutPanel1.Size = ScrollSize1;
                    }
                    panel1.ResumeLayout();

                    bool temp = ShouldAnimate;
                    ShouldAnimate = false;
                    LoadPosition(MainLine.MovesList[0], false);
                    ShouldAnimate = temp;

                    if (focusLabel != null)
                    {
                        focusLabel.ForeColor = (focusLabel.Tag == MainLine ?
                            NotationMainForeColor : NotationSubForeColor);
                        focusLabel = null;
                    }
                    //MessageBox.Show((DateTime.Now - dt1).Milliseconds.ToString());
                }
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                String str = "There was an error in pasting the game. Try again later"
                    + "\n Error Message: The Clipboard could not be cleared. "
                    + "\n This typically occurs when the Clipboard"
                    +  "is being used by another process.";
                MessageBox.Show(str);
            }
        }
        private void DisplayCommentForm()
        {
            if (CommentForm == null)
            {
                InitializeCommentForm();
            }

            Move move = CurrentPosition.LastMovePlayed;
            if (move == null)
                return;
            LongCommentBox.Text = move.Comment.LongComment;
            switch (move.Comment.MoveComment)
            {
                case MoveComment.None:
                    NoMoveComment.Checked = true;
                    break;
                case MoveComment.InterestingMove:
                    InterestingMove.Checked = true;
                    break;
                case MoveComment.DubiousMove:
                    DubiousMove.Checked = true;
                    break;
                case MoveComment.StrongMove:
                    StrongMove.Checked = true;
                    break;
                case MoveComment.WeakMove:
                    WeakMove.Checked = true;
                    break;
                case MoveComment.GreatMove:
                    GreatMove.Checked = true;
                    break;
                case MoveComment.TerribleMove:
                    TerribleMove.Checked = true;
                    break;
            }
            switch (move.Comment.PositionComment)
            {
                case PositionComment.None:
                    NoPositionComment.Checked = true;
                    break;
                case PositionComment.EqualPosition:
                    EqualPosition.Checked = true;
                    break;
                case PositionComment.SlightWhiteAdv:
                    SlightWhiteAdv.Checked = true;
                    break;
                case PositionComment.SlightBlackAdv:
                    SlightBlackAdv.Checked = true;
                    break;
                case PositionComment.ModerateWhiteAdv:
                    ModerateWhiteAdv.Checked = true;
                    break;
                case PositionComment.ModerateBlackAdv:
                    ModerateBlackAdv.Checked = true;
                    break;
                case PositionComment.DecisiveWhiteAdv:
                    DecisiveWhiteAdv.Checked = true;
                    break;
                case PositionComment.DecisiveBlackAdv:
                    DecisiveBlackAdv.Checked = true;
                    break;
            }
            CommentForm.Text = "Add Comment to move: ";
            CommentForm.Text += move.MoveNo.ToString();
            CommentForm.Text += (move.PieceMoving.Side == Piece.PieceSide.White ?
                ". " : "... ");
            CommentForm.Text += move.ShortNotation;
            
            CommentForm.ShowDialog(this);
        }
        private void InitializeCommentForm()
        {
            Button EnterKey = new Button();
            Button CancelKey = new Button();
            EnterKey.Click += EnterKey_Click;

            CommentForm = new Form();
            Font CommentFont = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Regular);
            Font CommentFont2 = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold);
            CommentForm.StartPosition = FormStartPosition.CenterParent;
            CommentForm.Size = new System.Drawing.Size(490, 450);
            CommentForm.ShowIcon = false;
            CommentForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            CommentForm.MinimizeBox = false;
            CommentForm.MaximizeBox = false;
            CommentForm.AcceptButton = EnterKey;
            CommentForm.CancelButton = CancelKey;

            LongCommentBox = new TextBox();
            NoMoveComment = new RadioButton();
            StrongMove = new RadioButton();
            WeakMove = new RadioButton();
            InterestingMove = new RadioButton();
            DubiousMove = new RadioButton();
            GreatMove = new RadioButton();
            TerribleMove = new RadioButton();
            NoPositionComment = new RadioButton();
            EqualPosition = new RadioButton();
            ModerateWhiteAdv = new RadioButton();
            ModerateBlackAdv = new RadioButton();
            SlightWhiteAdv = new RadioButton();
            SlightBlackAdv = new RadioButton();
            DecisiveWhiteAdv = new RadioButton();
            DecisiveBlackAdv = new RadioButton();

            Label LongCommentLabel = new Label();
            Label QuickCommentLabel = new Label();

            NoMoveComment.Font = CommentFont; StrongMove.Font = CommentFont;
            WeakMove.Font = CommentFont; InterestingMove.Font = CommentFont;
            DubiousMove.Font = CommentFont; GreatMove.Font = CommentFont;
            TerribleMove.Font = CommentFont; NoPositionComment.Font = CommentFont;
            EqualPosition.Font = CommentFont; ModerateBlackAdv.Font = CommentFont;
            ModerateWhiteAdv.Font = CommentFont; SlightBlackAdv.Font = CommentFont;
            SlightWhiteAdv.Font = CommentFont; DecisiveBlackAdv.Font = CommentFont;
            DecisiveWhiteAdv.Font = CommentFont;

            NoMoveComment.Text = "None";
            NoPositionComment.Text = "None";
            StrongMove.Text = "!      (Strong Move)";
            WeakMove.Text = "?      (Weak Move)";
            InterestingMove.Text = "!?     (Interesting Move)";
            DubiousMove.Text = "?!     (Dubious Move)";
            GreatMove.Text = "!!     (Great Move)";
            TerribleMove.Text = "??     (Terrible Move)";
            EqualPosition.Text = "=        (Equal/Quiet Position)";
            ModerateBlackAdv.Text = "-/+     (Moderate Black Advantage)";
            ModerateWhiteAdv.Text = "+/-     (Moderate White Advantage)";
            SlightBlackAdv.Text = "=+      (Slight Black Advantage)";
            SlightWhiteAdv.Text = "+=      (Slight White Advantage)";
            DecisiveBlackAdv.Text = "-+      (Decisive Black Advantage)";
            DecisiveWhiteAdv.Text = "+-      (Decisive White Advantage)";
            LongCommentLabel.Text = "Long Comment";
            QuickCommentLabel.Text = "Quick Comment";

            NoMoveComment.AutoSize = true; NoPositionComment.AutoSize = true;
            StrongMove.AutoSize = true; EqualPosition.AutoSize = true;
            WeakMove.AutoSize = true; ModerateBlackAdv.AutoSize = true;
            InterestingMove.AutoSize = true; ModerateWhiteAdv.AutoSize = true;
            DubiousMove.AutoSize = true; SlightWhiteAdv.AutoSize = true;
            GreatMove.AutoSize = true; SlightBlackAdv.AutoSize = true;
            TerribleMove.AutoSize = true; DecisiveWhiteAdv.AutoSize = true;
            DecisiveBlackAdv.AutoSize = true; QuickCommentLabel.AutoSize = true;
            LongCommentLabel.AutoSize = true;

            NoMoveComment.Location = new Point(5, 20);
            GreatMove.Location = new Point(5, 40);
            StrongMove.Location = new Point(5, 60);
            InterestingMove.Location = new Point(5, 80);
            DubiousMove.Location = new Point(5, 100);
            WeakMove.Location = new Point(5, 120);
            TerribleMove.Location = new Point(5, 140);

            NoPositionComment.Location = new Point(5, 20);
            DecisiveWhiteAdv.Location = new Point(5, 40);
            ModerateWhiteAdv.Location = new Point(5, 60);
            SlightWhiteAdv.Location = new Point(5, 80);
            EqualPosition.Location = new Point(5, 100);
            SlightBlackAdv.Location = new Point(5, 120);
            ModerateBlackAdv.Location = new Point(5, 140);
            DecisiveBlackAdv.Location = new Point(5, 160);

            GroupBox PositionRadioButtons = new GroupBox();
            GroupBox MoveRadioButtons = new GroupBox();

            MoveRadioButtons.Controls.Add(NoMoveComment);
            MoveRadioButtons.Controls.Add(StrongMove);
            MoveRadioButtons.Controls.Add(WeakMove);
            MoveRadioButtons.Controls.Add(InterestingMove);
            MoveRadioButtons.Controls.Add(DubiousMove);
            MoveRadioButtons.Controls.Add(GreatMove);
            MoveRadioButtons.Controls.Add(TerribleMove);
            MoveRadioButtons.Text = "ON MOVE";
            MoveRadioButtons.Font = CommentFont2;
            MoveRadioButtons.Location = new Point(10, 30);
            MoveRadioButtons.Size = new System.Drawing.Size(180, 185);

            PositionRadioButtons.Controls.Add(NoPositionComment);
            PositionRadioButtons.Controls.Add(EqualPosition);
            PositionRadioButtons.Controls.Add(ModerateBlackAdv);
            PositionRadioButtons.Controls.Add(ModerateWhiteAdv);
            PositionRadioButtons.Controls.Add(SlightBlackAdv);
            PositionRadioButtons.Controls.Add(SlightWhiteAdv);
            PositionRadioButtons.Controls.Add(DecisiveBlackAdv);
            PositionRadioButtons.Controls.Add(DecisiveWhiteAdv);
            PositionRadioButtons.Text = "ON POSITION";
            PositionRadioButtons.Font = CommentFont2;
            PositionRadioButtons.Location = new Point(200, 30);
            PositionRadioButtons.Size = new Size(255, 185);

            LongCommentBox.Size = new System.Drawing.Size(443, 100);
            LongCommentBox.Multiline = true;
            LongCommentBox.Location = new Point(10, 250);
            LongCommentBox.Font = new Font("Segoe UI", 12F, FontStyle.Regular);

            EnterKey.Text = "OK";
            CancelKey.Text = "Cancel";
            EnterKey.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            CancelKey.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            EnterKey.Location = new Point(400, 360);
            CancelKey.Location = new Point(300, 360);
            EnterKey.Size = new System.Drawing.Size(55, 40);
            CancelKey.Size = new System.Drawing.Size(80, 40);

            QuickCommentLabel.Location = new Point(150, 5);
            QuickCommentLabel.Font = CommentFont2;
            LongCommentLabel.Location = new Point(150, 220);
            LongCommentLabel.Font = CommentFont2;

            CommentForm.Controls.Add(MoveRadioButtons);
            CommentForm.Controls.Add(PositionRadioButtons);
            CommentForm.Controls.Add(LongCommentBox);
            CommentForm.Controls.Add(EnterKey);
            CommentForm.Controls.Add(CancelKey);
            CommentForm.Controls.Add(QuickCommentLabel);
            CommentForm.Controls.Add(LongCommentLabel);
        }
        void EnterKey_Click(object sender, EventArgs e)
        {
            Label label = LabelClicked != null ? LabelClicked : focusLabel;
            if (label == null)
            {
                CommentForm.Close();
                return;
            }
            Move move = CurrentPosition.LastMovePlayed;
            if (NoMoveComment.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.None)
                    RemoveLabelAnnotation(move, "move");
                move.Comment.MoveComment = MoveComment.None;
                if (move.Comment.LongComment == ""
                    && move.Comment.PositionComment == PositionComment.None)
                    label.AccessibleDescription = "";
            }
            if (StrongMove.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.StrongMove)
                {
                    RemoveLabelAnnotation(move, "move");
                    InsertLabelAnnotation(label, move, "move", "!");
                }
                move.Comment.MoveComment = MoveComment.StrongMove;
                label.AccessibleDescription = "comment";
            }
            if (WeakMove.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.WeakMove)
                {
                    RemoveLabelAnnotation(move, "move");
                    InsertLabelAnnotation(label, move, "move", "?");
                }
                move.Comment.MoveComment = MoveComment.WeakMove;
                label.AccessibleDescription = "comment";
            }
            if (InterestingMove.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.InterestingMove)
                {
                    RemoveLabelAnnotation(move, "move");
                    InsertLabelAnnotation(label, move, "move", "!?");
                }
                move.Comment.MoveComment = MoveComment.InterestingMove;
                label.AccessibleDescription = "comment";
            }
            if (DubiousMove.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.DubiousMove)
                {
                    RemoveLabelAnnotation(move, "move");
                    InsertLabelAnnotation(label, move, "move", "?!");
                }
                move.Comment.MoveComment = MoveComment.DubiousMove;
                label.AccessibleDescription = "comment";
            }
            if (GreatMove.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.GreatMove)
                {
                    RemoveLabelAnnotation(move, "move");
                    InsertLabelAnnotation(label, move, "move", "!!");
                }
                move.Comment.MoveComment = MoveComment.GreatMove;
                label.AccessibleDescription = "comment";
            }
            if (TerribleMove.Checked)
            {
                if (move.Comment.MoveComment != MoveComment.TerribleMove)
                {
                    RemoveLabelAnnotation(move, "move");
                    InsertLabelAnnotation(label, move, "move", "??");
                }
                move.Comment.MoveComment = MoveComment.TerribleMove;
                label.AccessibleDescription = "comment";
            }

            if (NoPositionComment.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.None)
                    RemoveLabelAnnotation(move, "position");
                move.Comment.PositionComment = PositionComment.None;
                if (move.Comment.LongComment == ""
                    && move.Comment.MoveComment == MoveComment.None)
                    label.AccessibleDescription = "";
            }
            if (EqualPosition.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.EqualPosition)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t =");
                }
                move.Comment.PositionComment = PositionComment.EqualPosition;
                label.AccessibleDescription = "comment";
            }
            if (SlightWhiteAdv.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.SlightWhiteAdv)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t +=");
                }
                move.Comment.PositionComment = PositionComment.SlightWhiteAdv;
                label.AccessibleDescription = "comment";
            }
            if (SlightBlackAdv.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.SlightBlackAdv)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t =+");
                }
                move.Comment.PositionComment = PositionComment.SlightBlackAdv;
                label.AccessibleDescription = "comment";
            }
            if (ModerateWhiteAdv.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.ModerateWhiteAdv)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t +/-");
                }
                move.Comment.PositionComment = PositionComment.ModerateWhiteAdv;
                label.AccessibleDescription = "comment";
            }
            if (ModerateBlackAdv.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.ModerateBlackAdv)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t -/+");
                }
                move.Comment.PositionComment = PositionComment.ModerateBlackAdv;
                label.AccessibleDescription = "comment";
            }
            if (DecisiveWhiteAdv.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.DecisiveWhiteAdv)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t +-");
                }
                move.Comment.PositionComment = PositionComment.DecisiveWhiteAdv;
                label.AccessibleDescription = "comment";
            }
            if (DecisiveBlackAdv.Checked)
            {
                if (move.Comment.PositionComment != PositionComment.DecisiveBlackAdv)
                {
                    RemoveLabelAnnotation(move, "position");
                    InsertLabelAnnotation(label, move, "position", "\t -+");
                }
                move.Comment.PositionComment = PositionComment.DecisiveBlackAdv;
                label.AccessibleDescription = "comment";
            }
            if (LongCommentBox.Text != move.Comment.LongComment)
            {
                if (LongCommentBox.Text == "")
                {
                    if (move.Comment.MoveComment == MoveComment.None
                        && move.Comment.PositionComment == PositionComment.None)
                        label.AccessibleDescription = "";
                    if (move.Comment.LongComment != "")
                        label.Text = label.Text.Remove(
                        label.Text.LastIndexOf("\t"));
                }
                else
                {
                    if (move.Comment.LongComment != "")
                    {
                        label.Text = label.Text.Remove(
                            label.Text.LastIndexOf("\t"));
                    }
                    label.Text += "\t " + LongCommentBox.Text;
                    move.Comment.LongComment = LongCommentBox.Text;
                }
            }
            CommentForm.Close();
        }
        private void InsertLabelAnnotation(Label label, Move move, string p, string anno)
        {
            if (p == "move")
            {
                int x = move.ShortNotation.Length;
                label.Text = 
                label.Text.Insert(label.Text.IndexOf(move.ShortNotation) + x, anno);
            }
            if (p == "position")
            {
                int x;
                if (move.Comment.LongComment == "")
                    x = label.Text.Length;
                else
                    x = label.Text.IndexOf("\t");
                label.Text = label.Text.Insert(x, anno);
            }
        }
        private void InsertLabelAnnotation(Label label, Comment comment)
        {
            if (comment.MoveComment != MoveComment.None)
            {
                label.AccessibleDescription = "comment";
                switch (comment.MoveComment)
                {
                    case MoveComment.InterestingMove:
                        label.Text += "!?";
                        break;
                    case MoveComment.DubiousMove:
                        label.Text += "?!";
                        break;
                    case MoveComment.StrongMove:
                        label.Text += "!";
                        break;
                    case MoveComment.WeakMove:
                        label.Text += "?";
                        break;
                    case MoveComment.GreatMove:
                        label.Text += "!!";
                        break;
                    case MoveComment.TerribleMove:
                        label.Text += "??";
                        break;
                }
            }
            if (comment.PositionComment != PositionComment.None)
            {
                label.AccessibleDescription = "comment";
                switch (comment.PositionComment)
                {
                    case PositionComment.EqualPosition:
                        label.Text += "\t=";
                        break;
                    case PositionComment.SlightWhiteAdv:
                        label.Text += "\t+=";
                        break;
                    case PositionComment.SlightBlackAdv:
                        label.Text += "\t=+";
                        break;
                    case PositionComment.ModerateWhiteAdv:
                        label.Text += "\t+/-";
                        break;
                    case PositionComment.ModerateBlackAdv:
                        label.Text += "\t-/+";
                        break;
                    case PositionComment.DecisiveWhiteAdv:
                        label.Text += "\t+-";
                        break;
                    case PositionComment.DecisiveBlackAdv:
                        label.Text += "\t-+";
                        break;
                }
            }
            if (comment.LongComment != "")
            {
                comment.LongComment = comment.LongComment.Replace('\n', ' ');
                comment.LongComment = comment.LongComment.Replace('\r', ' ');
                label.AccessibleDescription = "comment";
                label.Text += "\t " + comment.LongComment;
            }
        }
        private void RemoveLabelAnnotation(Move move, string p)
        {
            if (p == "move")
            {
                switch (move.Comment.MoveComment)
                {
                    case MoveComment.InterestingMove:
                        LabelClicked.Text = 
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("!?"), 2);
                        break;
                    case MoveComment.DubiousMove:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("?!"), 2);
                        break;
                    case MoveComment.StrongMove:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("!"), 1);
                        break;
                    case MoveComment.WeakMove:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("?"), 1);
                        break;
                    case MoveComment.GreatMove:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("!!"), 2);
                        break;
                    case MoveComment.TerribleMove:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("??"), 2);
                        break;
                }
            }
            if (p == "position")
            {
                switch (move.Comment.PositionComment)
                {
                    case PositionComment.EqualPosition:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t ="), 3);
                        break;
                    case PositionComment.SlightWhiteAdv:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t +="), 4);
                        break;
                    case PositionComment.SlightBlackAdv:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t =+"), 4);
                        break;
                    case PositionComment.ModerateWhiteAdv:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t +/-"), 5);
                        break;
                    case PositionComment.ModerateBlackAdv:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t -/+"), 5);
                        break;
                    case PositionComment.DecisiveWhiteAdv:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t +-"), 4);
                        break;
                    case PositionComment.DecisiveBlackAdv:
                        LabelClicked.Text =
                            LabelClicked.Text.Remove(LabelClicked.Text.IndexOf("\t -+"), 4);
                        break;
                }
            }
        }
        private void DisplayGameDetails()
        {
            if (GameDetailForm == null)
            {
                InitializeDetailsForm();
            }

            if (gameDetails.BlackPlayer.Rating > 0)
                BlackEloBox.Text = gameDetails.BlackPlayer.Rating.ToString();
            else
                BlackEloBox.Text = "";
            if (gameDetails.BlackPlayer.Name != "?")
                BlackNameBox.Text = gameDetails.BlackPlayer.Name;
            else
                BlackNameBox.Text = "";
            if (gameDetails.Date == "?")
                DateBox.Text = "";
            else
                DateBox.Text = gameDetails.Date;
            if (gameDetails.ECO != "?")
                ECOBox.Text = gameDetails.ECO;
            else
                ECOBox.Text = "";
            if (gameDetails.Event != "?")
                EventBox.Text = gameDetails.Event;
            else
                EventBox.Text = "";
            switch (gameDetails.Result)
            {
                case GameDetails.Outcome.WhiteWin:
                    WhiteWinButton.Checked = true;
                    break;
                case GameDetails.Outcome.BlackWin:
                    BlackWinButton.Checked = true;
                    break;
                case GameDetails.Outcome.Draw:
                    DrawButton.Checked = true;
                    break;
                case GameDetails.Outcome.NotAvailable:
                    WhiteWinButton.Checked = false;
                    BlackWinButton.Checked = false;
                    DrawButton.Checked = false;
                    break;
            }
            if (gameDetails.Round > 0)
                RoundBox.Text = gameDetails.Round.ToString();
            if (gameDetails.Site != "?")
                SiteBox.Text = gameDetails.Site;
            else
                SiteBox.Text = "";
            if (gameDetails.WhitePlayer.Rating > 0)
                WhiteEloBox.Text = gameDetails.WhitePlayer.Rating.ToString();
            else
                WhiteEloBox.Text = "";
            if (gameDetails.WhitePlayer.Name != "?")
                WhiteNameBox.Text = gameDetails.WhitePlayer.Name;
            else
                WhiteNameBox.Text = "";
            GameDetailForm.ShowDialog(this);
        }
        private void InitializeDetailsForm()
        {
            Font LabelFont = new System.Drawing.Font("Segoe UI", 10.5F, FontStyle.Regular);
            Font LabelFont2 = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold);
            GameDetailForm = new Form();
            GameDetailForm.StartPosition = FormStartPosition.CenterParent;
            GameDetailForm.Size = new System.Drawing.Size(400, 350);
            GameDetailForm.ShowIcon = false;
            GameDetailForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            GameDetailForm.MinimizeBox = false;
            GameDetailForm.MaximizeBox = false;
            GameDetailForm.Text = "Game Details";

            WhiteNameBox = new TextBox();
            WhiteEloBox = new TextBox();
            BlackNameBox = new TextBox();
            BlackEloBox = new TextBox();
            EventBox = new TextBox();
            SiteBox = new TextBox();
            RoundBox = new TextBox();
            DateBox = new TextBox();
            ECOBox = new TextBox();

            WhiteWinButton = new RadioButton();
            BlackWinButton = new RadioButton();
            DrawButton = new RadioButton();

            Label WhiteLabel = new Label(), WhiteEloLabel = new Label(), BlackLabel = new Label(),
                BlackEloLabel = new Label(), EventLabel = new Label(), SiteLabel = new Label(),
                RoundLabel = new Label(), DateLabel = new Label(), ECOLabel = new Label(),
                ResultLabel = new Label();

            GameDetailForm.Controls.Add(DrawButton);
            GameDetailForm.Controls.Add(WhiteWinButton);
            GameDetailForm.Controls.Add(BlackWinButton);
            GameDetailForm.Controls.Add(WhiteNameBox);
            GameDetailForm.Controls.Add(WhiteEloBox);
            GameDetailForm.Controls.Add(BlackNameBox);
            GameDetailForm.Controls.Add(BlackEloBox);
            GameDetailForm.Controls.Add(EventBox);
            GameDetailForm.Controls.Add(SiteBox);
            GameDetailForm.Controls.Add(RoundBox);
            GameDetailForm.Controls.Add(DateBox);
            GameDetailForm.Controls.Add(ECOBox);
            GameDetailForm.Controls.Add(WhiteEloLabel);
            GameDetailForm.Controls.Add(WhiteLabel);
            GameDetailForm.Controls.Add(BlackEloLabel);
            GameDetailForm.Controls.Add(BlackLabel);
            GameDetailForm.Controls.Add(EventLabel);
            GameDetailForm.Controls.Add(SiteLabel);
            GameDetailForm.Controls.Add(RoundLabel);
            GameDetailForm.Controls.Add(DateLabel);
            GameDetailForm.Controls.Add(ECOLabel);
            GameDetailForm.Controls.Add(ResultLabel);

            WhiteLabel.Text = "White";
            BlackLabel.Text = "Black";
            WhiteEloLabel.Text = "Rating";
            BlackEloLabel.Text = "Rating";
            EventLabel.Text = "Event";
            SiteLabel.Text = "Site";
            RoundLabel.Text = "Round";
            DateLabel.Text = "Date";
            ECOLabel.Text = "ECO";
            ResultLabel.Text = "Game Result";
            DrawButton.Text = "Draw";
            WhiteWinButton.Text = "1-0";
            BlackWinButton.Text = "0-1";

            WhiteLabel.AutoSize = true; WhiteEloLabel.AutoSize = true;
            BlackLabel.AutoSize = true; BlackEloLabel.AutoSize = true;
            EventLabel.AutoSize = true; SiteLabel.AutoSize = true;
            RoundLabel.AutoSize = true; DateLabel.AutoSize = true;
            ECOLabel.AutoSize = true; ResultLabel.AutoSize = true;
            WhiteWinButton.AutoSize = true; DrawButton.AutoSize = true;
            BlackWinButton.AutoSize = true;
            WhiteLabel.Font = LabelFont2; BlackLabel.Font = LabelFont2;
            WhiteEloLabel.Font = LabelFont2; BlackEloLabel.Font = LabelFont2;
            EventLabel.Font = LabelFont2; SiteLabel.Font = LabelFont2;
            RoundLabel.Font = LabelFont2; DateLabel.Font = LabelFont2;
            ECOLabel.Font = LabelFont2; ResultLabel.Font = LabelFont2;
            WhiteNameBox.Font = LabelFont; WhiteEloBox.Font = LabelFont;
            BlackNameBox.Font = LabelFont; BlackEloBox.Font = LabelFont;
            EventBox.Font = LabelFont; SiteBox.Font = LabelFont;
            RoundBox.Font = LabelFont; DateBox.Font = LabelFont;
            ECOBox.Font = LabelFont;

            WhiteLabel.Location = new Point(5, 20);
            WhiteNameBox.Location = new Point(55, 15);
            WhiteNameBox.Size = new System.Drawing.Size(200, 40);
            WhiteEloLabel.Location = new Point(260, 20);
            WhiteEloBox.Location = new Point(315, 15);
            WhiteEloBox.Size = new System.Drawing.Size(45, 40);

            BlackLabel.Location = new Point(7, 50);
            BlackNameBox.Location = new Point(55, 45);
            BlackNameBox.Size = new System.Drawing.Size(200, 40);
            BlackEloLabel.Location = new Point(260, 50);
            BlackEloBox.Location = new Point(315, 45);
            BlackEloBox.Size = new System.Drawing.Size(45, 40);

            EventLabel.Location = new Point(5, 80);
            EventBox.Location = new Point(55, 75);
            EventBox.Size = new System.Drawing.Size(306, 40);

            SiteLabel.Location = new Point(15, 110);
            SiteBox.Location = new Point(55, 105);
            SiteBox.Size = new System.Drawing.Size(306, 40);

            DateLabel.Location = new Point(10, 140);
            DateBox.Location = new Point(55, 135);
            DateBox.Size = new System.Drawing.Size(100, 40);

            RoundLabel.Location = new Point(5, 170);
            RoundBox.Location = new Point(55, 165);
            RoundBox.Size = new System.Drawing.Size(35, 40);

            ECOLabel.Location = new Point(15, 200);
            ECOBox.Location = new Point(55, 195);
            ECOBox.Size = new System.Drawing.Size(35, 40);

            ResultLabel.Location = new Point(220, 170);
            WhiteWinButton.Location = new Point(200, 200);
            DrawButton.Location = new Point(250, 200);
            BlackWinButton.Location = new Point(310, 200);

            Button AcceptButton1 = new Button(), CancelButton1 = new Button();
            GameDetailForm.Controls.Add(AcceptButton1);
            GameDetailForm.Controls.Add(CancelButton1);
            AcceptButton1.Text = "OK";
            CancelButton1.Text = "Cancel";
            AcceptButton1.Location = new Point(300, 250);
            AcceptButton1.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            AcceptButton1.Size = new System.Drawing.Size(60, 40);
            CancelButton1.Location = new Point(200, 250);
            CancelButton1.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            CancelButton1.Size = new System.Drawing.Size(80, 40);
            GameDetailForm.CancelButton = CancelButton1;
            GameDetailForm.AcceptButton = AcceptButton1;
            AcceptButton1.Click += AcceptButton1_Click;
            CancelButton1.Click += CancelButton1_Click;

            WhiteNameBox.MaxLength = 20;
            BlackNameBox.MaxLength = 20;
            WhiteEloBox.MaxLength = 5;
            BlackEloBox.MaxLength = 5;
            EventBox.MaxLength = (SSEventV.Tag as String).Length;
            RoundBox.MaxLength = 2;
            ECOBox.MaxLength = 3;
        }
        void CancelButton1_Click(object sender, EventArgs e)
        {
            GameDetailForm.Close();
        }
        void AcceptButton1_Click(object sender, EventArgs e)
        { 
            int a;
            String dt;
            if (TryConvertDate(DateBox.Text, out dt))
                gameDetails.Date = dt;
            if (WhiteWinButton.Checked)
                gameDetails.Result = GameDetails.Outcome.WhiteWin;
            if (DrawButton.Checked)
                gameDetails.Result = GameDetails.Outcome.Draw;
            if (BlackWinButton.Checked)
                gameDetails.Result = GameDetails.Outcome.BlackWin;
            if (int.TryParse(RoundBox.Text, out a))
                gameDetails.Round = a;
            gameDetails.ECO = ECOBox.Text;
            gameDetails.Event = EventBox.Text;
            gameDetails.Site = SiteBox.Text;
            int x = -1, y = -1;
            if ((int.TryParse(BlackEloBox.Text, out y) && gameDetails.BlackPlayer.Rating != y)
                || gameDetails.BlackPlayer.Name != BlackNameBox.Text
                || (int.TryParse(WhiteEloBox.Text, out x) && gameDetails.WhitePlayer.Rating != x)
                || gameDetails.WhitePlayer.Name != WhiteNameBox.Text)
            {
                gameDetails.WhitePlayer = new User(WhiteNameBox.Text, x < 0 ? gameDetails.WhitePlayer.Rating : x);
                gameDetails.BlackPlayer = new User(BlackNameBox.Text, y < 0 ? gameDetails.BlackPlayer.Rating : y);
            }
            GameDetailForm.Close();
            ShowGameDetails();
        }
        private bool TryConvertDate(string p, out String result)
        {
            DateTime dt;
            if (DateTime.TryParse(p, out dt))
            {
                result = dt.Year + "." + dt.Month + "." + dt.Day;
                gameDetails.RegularDateString = dt.Year + "/" + dt.Month + "/" + dt.Day;
                return true;
            }
            bool IsReached = false;
            for (int i = 0; i < p.Length; i++)
            {
                if (IsReached && !char.IsNumber(p[i]) && i == 4)
                {
                    result = p.Substring(0, 4) + ".?.?";
                    gameDetails.RegularDateString = p.Substring(0, 4) + "/?/?";
                    return true;
                }
                if (char.IsNumber(p[i]))
                {
                    if (i == 3)
                        if (p.Length < 5)
                        {
                            IsReached = true;
                            result = p.Substring(0, 4) +".?.?";
                            gameDetails.RegularDateString = p.Substring(0, 4) + "/?/?";
                            return true;
                        }
                        else
                        {
                            IsReached = true;
                        }

                }
                    
                else
                    break;
            }
            result = "?";
            gameDetails.RegularDateString = "";
            return false;
        }
        private void CopyGame()
        {
            if (gameDetails.Date == "?")
                gameDetails.Date = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day;
            String buffer = "";
            buffer += "[Event \"" +gameDetails.Event +"\"]" + Environment.NewLine;
            buffer += "[Site \"" + gameDetails.Site + "\"]" + Environment.NewLine;
            buffer += "[Date \"" + gameDetails.Date + "\"]" + Environment.NewLine;
            buffer += "[Round \"" + (gameDetails.Round > 0 ? gameDetails.Round.ToString() : "?")
                + "\"]" + Environment.NewLine;
            buffer += "[White \"" + gameDetails.WhitePlayer.Name + "\"]" + Environment.NewLine;
            buffer += "[Black \"" + gameDetails.BlackPlayer.Name + "\"]" + Environment.NewLine;
            buffer += "[WhiteElo \"" + (gameDetails.WhitePlayer.Rating > 0 ? gameDetails.WhitePlayer.Rating.ToString() : "?")
                + "\"]" + Environment.NewLine;
            buffer += "[BlackElo \"" + (gameDetails.BlackPlayer.Rating > 0 ? gameDetails.BlackPlayer.Rating.ToString() : "?")
                + "\"]" + Environment.NewLine;
            buffer += "[Result \"";
            switch (gameDetails.Result)
            {
                case GameDetails.Outcome.WhiteWin:
                    buffer += "1-0";
                    break;
                case GameDetails.Outcome.BlackWin:
                    buffer += "0-1";
                    break;
                case GameDetails.Outcome.Draw:
                    buffer += "1/2-1/2";
                    break;
                case GameDetails.Outcome.NotAvailable:
                    buffer += "*";
                    break;
                default:
                    break;
            }
            buffer += "\"]" + Environment.NewLine;
            buffer += "[ECO \"" + gameDetails.ECO + "\"]" + Environment.NewLine 
                + Environment.NewLine;

            bool InsertNumbering = false;
            foreach (var item in MainLine.MovesList)
            {
                if (item != MainLine.MovesList[0])
                {
                    if (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                        || InsertNumbering)
                    {
                        InsertNumbering = false;
                        buffer += item.LastMovePlayed.MoveNo;
                        buffer += (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                            ? ". " : "... ");
                    }
                    buffer += item.LastMovePlayed.ShortNotation +" ";

                    buffer += InsertAnnotation(item);

                    if (item.VariationHolders != null
                        && item.VariationHolders.Count > 0)
                    {
                        buffer += Environment.NewLine;
                        buffer += CopyVariation(item.VariationHolders);
                        buffer += Environment.NewLine;
                        InsertNumbering = true;
                    }
                }
            }
            switch (gameDetails.Result)
            {
                case GameDetails.Outcome.WhiteWin:
                    buffer += "1-0";
                    break;
                case GameDetails.Outcome.BlackWin:
                    buffer += "0-1";
                    break;
                case GameDetails.Outcome.Draw:
                    buffer += "1/2-1/2";
                    break;
                case GameDetails.Outcome.NotAvailable:
                    buffer += "*";
                    break;
            }
            try
            {
                Clipboard.SetText(buffer);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                String str = "There was an error in pasting the game. Try again later"
                    + "\n Error Message: The Clipboard could not be cleared. "
                    + "This typically occurs when the Clipboard"
                    + "is being used by another process.";
                MessageBox.Show(str);
            }
        }
        private static string InsertAnnotation(Position item)
        {
            String buffer = "";
            switch (item.LastMovePlayed.Comment.MoveComment)
            {
                case MoveComment.None:
                    break;
                case MoveComment.InterestingMove:
                    buffer += " $5 ";
                    break;
                case MoveComment.DubiousMove:
                    buffer += " $6 ";
                    break;
                case MoveComment.StrongMove:
                    buffer += " $1 ";
                    break;
                case MoveComment.WeakMove:
                    buffer += " $2 ";
                    break;
                case MoveComment.GreatMove:
                    buffer += " $3 ";
                    break;
                case MoveComment.TerribleMove:
                    buffer += " $4 ";
                    break;
            }
            switch (item.LastMovePlayed.Comment.PositionComment)
            {
                case PositionComment.None:
                    break;
                case PositionComment.EqualPosition:
                    buffer += " $11 ";
                    break;
                case PositionComment.SlightWhiteAdv:
                    buffer += " $14 ";
                    break;
                case PositionComment.SlightBlackAdv:
                    buffer += " $15 ";
                    break;
                case PositionComment.ModerateWhiteAdv:
                    buffer += " $16 ";
                    break;
                case PositionComment.ModerateBlackAdv:
                    buffer += " $17 ";
                    break;
                case PositionComment.DecisiveWhiteAdv:
                    buffer += " $18 ";
                    break;
                case PositionComment.DecisiveBlackAdv:
                    buffer += " $19 ";
                    break;
            }
            if (item.LastMovePlayed.Comment.LongComment != "")
            {
                buffer += " {" + item.LastMovePlayed.Comment.LongComment + "} ";
            }
            return buffer;
        }
        private String CopyVariation(List<VariationHolder> VarList)
        {
            String buffer = "";
            Stack<Tuple<VariationHolder, int>> tupleStack = 
                new Stack<Tuple<VariationHolder, int>>();
            for (int i = VarList.Count - 1; i >= 0; i--)
            {
                tupleStack.Push(Tuple.Create(VarList[i], 0));
            }

            bool InsertNumbering = true, IsNewVar = true;
            while (tupleStack.Count > 0)
            {
                if (IsNewVar)
                    buffer += " (";
                IsNewVar = false;
                var CurrentNode = tupleStack.Pop();
                foreach (var item in CurrentNode.Item1.MovesList)
                {
                    if (CurrentNode.Item2 != 0 &&
                        CurrentNode.Item1.MovesList.IndexOf(item) < CurrentNode.Item2)
                        continue;
                    if (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                        || InsertNumbering)
                    {
                        InsertNumbering = false;
                        buffer += item.LastMovePlayed.MoveNo;
                        buffer += (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                            ? ". " : "... ");
                    }
                    buffer += item.LastMovePlayed.ShortNotation +" ";
                    buffer += InsertAnnotation(item);

                    if (item.VariationHolders != null && item.VariationHolders.Count > 0)
                    {
                        int x = CurrentNode.Item1.MovesList.IndexOf(item) + 1;
                        tupleStack.Push(Tuple.Create(CurrentNode.Item1, x));
                        for (int i = item.VariationHolders.Count - 1; i >= 0; i--)
                        {
                            tupleStack.Push(Tuple.Create(item.VariationHolders[i], 0));
                        }
                        IsNewVar = true;
                        InsertNumbering = true;
                        break;
                    }
                }
                if (IsNewVar)
                    continue;
                buffer += ") ";
                InsertNumbering = true;
            }
            return buffer;
        }
        private void MoveVariationUp(Label LabelClicked)
        {
            if (LabelClicked == null && CurrentVariation == MainLine)
                return;
            else if (CurrentVariation != MainLine)
            {
                LabelClicked = focusLabel;
            }
            VariationHolder VarToPromote = (VariationHolder)LabelClicked.Tag;
            if (VarToPromote.ParentLine == null)
                return;
            VariationHolder tempVarHolder = new VariationHolder();
            tempVarHolder.MovesList = new List<Position>();
            bool IsReached = false;
            foreach (var item in VarToPromote.ParentLine.MovesList)
            {
                if (IsReached)
                {
                    tempVarHolder.MovesList.Add(item);
                    continue;
                }
                if (item == VarToPromote.ParentLine.MovesList[VarToPromote.ParentIndex])
                {
                    IsReached = true;
                }
            }

            foreach (var item in tempVarHolder.MovesList)
            {
                VarToPromote.ParentLine.MovesList.Remove(item);
            }

            VarToPromote.MovesList[0].VariationHolders = tempVarHolder.MovesList[0].VariationHolders;
            tempVarHolder.MovesList[0].VariationHolders = null;
            
            /*
            //   Populates tempVarHolder's ListOfParents field
            VariationHolder CurrentNode = tempVarHolder.ParentLine;
            bool ShouldContinue = true;
            while (ShouldContinue)
            {
                tempVarHolder.ListOfParents.Add(CurrentNode);
                if (CurrentNode.ParentLine != null)
                    CurrentNode = CurrentNode.ParentLine;
                else ShouldContinue = false;
            }
            */
            VarToPromote.ParentLine.MovesList.AddRange(VarToPromote.MovesList);
            foreach (var item in VarToPromote.MovesList)
            {
                if (item.VariationHolders != null)
                    foreach (var variation in item.VariationHolders)
                    {
                        variation.ParentLine = VarToPromote.ParentLine;
                        variation.ParentIndex = VarToPromote.ParentLine.MovesList.IndexOf(item) - 1;
                    }
            }

            foreach (var item in tempVarHolder.MovesList)
            {
                if (item.VariationHolders != null)
                    foreach (var variation in item.VariationHolders)
                    {
                        variation.ParentLine = VarToPromote;
                        variation.ParentIndex = tempVarHolder.MovesList.IndexOf(item) - 1;
                    }
            }
            VarToPromote.MovesList = tempVarHolder.MovesList;
            ReInsertLabels();
            if (CurrentPosition != MainLine.MovesList[0])
                flowLayoutPanel1.ScrollControlIntoView(focusLabel);

            if (flowLayoutPanel1.VerticalScroll.Visible && flowLayoutPanel1.Size != ScrollSize2)
            {
                flowLayoutPanel1.Size = ScrollSize2;
            }
            else if (!flowLayoutPanel1.VerticalScroll.Visible && flowLayoutPanel1.Size != ScrollSize1)
            {
                flowLayoutPanel1.Size = ScrollSize1;
            }
           
        }
        private void ReInsertLabels()
        {
            SSPanel.SuspendLayout();
            SSPanel.Tag = null;
            SSPanel.Controls.Clear();
            SSLastNumLabel = null;
            SSCurrentLabel = null;
            foreach (var item in MainLine.MovesList)
            {
                if (item == StartingPosition)
                    continue;
                Move move = item.LastMovePlayed;
                if (move == null)
                    continue;
                Label numLabel = null, notationLabel = new Label();
                if (SSLastNumLabel != null)
                {
                    if (SSLastNumLabel.Tag is Label)
                        ((Label)SSLastNumLabel.Tag).ForeColor = Color.Black;
                    else
                    {
                        ((Tuple<Label, Label>)SSLastNumLabel.Tag).Item2.ForeColor = Color.Black;
                    }
                }

                if (move.PieceMoving.Side == Piece.PieceSide.White)
                {
                    numLabel = new Label();
                    numLabel.Tag = notationLabel;
                    numLabel.Text = move.MoveNo.ToString();
                    if (SSLastNumLabel == null)
                    {
                        numLabel.Location = new Point(10, 1);
                    }
                    else
                    {
                        numLabel.Location = SSLastNumLabel.Location + new Size(0, 20);
                    }
                    SSLastNumLabel = numLabel;
                    numLabel.Font = new System.Drawing.Font("Calibri", 11F, FontStyle.Bold);
                    notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                    notationLabel.Location = numLabel.Location + new Size(30, -2);
                    notationLabel.Text = move.ShortNotation;
                    notationLabel.Tag = Tuple.Create(item, SSLastNumLabel);
                    numLabel.AutoSize = true;
                    notationLabel.AutoSize = true;
                    SSPanel.Controls.Add(numLabel);
                    SSPanel.Controls.Add(notationLabel);
                }
                else
                {
                    if (SSLastNumLabel != null)
                    {
                        SSLastNumLabel.Tag = Tuple.Create(SSLastNumLabel.Tag as Label, notationLabel);
                        notationLabel.AutoSize = true;
                        SSPanel.Controls.Add(notationLabel);
                        notationLabel.Text = move.ShortNotation;
                        notationLabel.Tag = Tuple.Create(item, SSLastNumLabel);
                        notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                        notationLabel.Location = SSLastNumLabel.Location + new Size(100, -2);
                    }
                    else
                    {
                        numLabel = new Label();
                        numLabel.Tag = notationLabel;
                        numLabel.Text = move.MoveNo.ToString();
                        numLabel.Location = new Point(10, 1);
                        SSLastNumLabel = numLabel;
                        numLabel.Font = new System.Drawing.Font("Calibri", 11F, FontStyle.Bold);
                        notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                        notationLabel.Location = numLabel.Location + new Size(30, -2);
                        notationLabel.Text = " ";
                        notationLabel.Tag = Tuple.Create(item, SSLastNumLabel);
                        numLabel.AutoSize = true;
                        notationLabel.AutoSize = true;
                        SSPanel.Controls.Add(numLabel);
                        SSPanel.Controls.Add(notationLabel);

                        SSLastNumLabel.Tag = Tuple.Create(SSLastNumLabel.Tag as Label, notationLabel);
                        notationLabel.AutoSize = true;
                        SSPanel.Controls.Add(notationLabel);
                        notationLabel.Text = move.ShortNotation;
                        notationLabel.Tag = Tuple.Create(CurrentPosition, SSLastNumLabel);
                        notationLabel.Font = new System.Drawing.Font("Segoe Print", 10F, FontStyle.Bold);
                        notationLabel.Location = SSLastNumLabel.Location + new Size(100, -2);
                    }
                }
                notationLabel.MouseDown += SSLabel_Click;
                SSCurrentLabel = notationLabel;
                List<Label> list = SSPanel.Tag == null ? new List<Label>() : SSPanel.Tag as List<Label>;
                list.Add(notationLabel);
                SSPanel.Tag = list;
            }

            SSCurrentLabel = null;
            int index = MainLine.MovesList.IndexOf(CurrentPosition);
            if (index > 0)
            {
                List<Label> list = SSPanel.Tag as List<Label>;
                SSCurrentLabel = list[index - 1];
            }
            if (SSPanel.Tag != null)
            {
                bool PASSED = false;
                List<Label> list = SSPanel.Tag as List<Label>;
                list.Reverse();
                foreach (var item in list)
                {
                    if (PASSED || item == SSCurrentLabel)
                    {
                        item.ForeColor = Color.Black;
                        PASSED = true;
                    }
                    else
                    {
                        item.ForeColor = Color.DimGray;
                    }
                }
                list.Reverse();
            }
            if (SSCurrentLabel != null)
                SSPanel.ScrollControlIntoView(SSCurrentLabel);
            SSPanel.ResumeLayout();


            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.Size = ScrollSize1;
            Label tempLabel = new Label();
            Stack<Tuple<VariationHolder, int>> tupleStack =
                new Stack<Tuple<VariationHolder, int>>();
            tupleStack.Push(Tuple.Create(MainLine, 1));
            bool InsertNumbering = true, IsNewVar = false;
            List<int> breakList = new List<int>();
            while (tupleStack.Count > 0)
            {
                var CurrentNode = tupleStack.Pop();
                if (CurrentNode.Item2 == 0)
                    IsNewVar = true;
                if (IsNewVar)
                {
                    tempLabel = new Label();
                    tempLabel.Name = "X";
                    tempLabel.Text = (CurrentNode.Item1.ParentLine == MainLine ? " [ " : "(");
                    if (CurrentNode.Item1.ParentLine == MainLine)
                        breakList.Add(flowLayoutPanel1.Controls.Count - 1);
                    tempLabel.ForeColor = (CurrentNode.Item1 == MainLine ? NotationMainForeColor :
                        NotationSubForeColor);
                    tempLabel.Tag = CurrentNode.Item1;
                    flowLayoutPanel1.Controls.Add(tempLabel);
                    tempLabel.Font = NotationFont;
                    tempLabel.AutoSize = true;
                }
                IsNewVar = false;

                foreach (var item in CurrentNode.Item1.MovesList)
                {
                    if (CurrentNode.Item2 != 0 &&
                        CurrentNode.Item1.MovesList.IndexOf(item) < CurrentNode.Item2)
                        continue;
                    tempLabel = new Label();
                    if (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                        || InsertNumbering)
                    {
                        InsertNumbering = false;
                        tempLabel.Margin = new System.Windows.Forms.Padding(0);
                        tempLabel.Text += item.LastMovePlayed.MoveNo;
                        tempLabel.Text += (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                            ? ". " : "... ");
                    }
                    tempLabel.Name = (item.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White
                        ? "W" : "B");
                    tempLabel.ContextMenuStrip = NotationContextMenu;
                    tempLabel.MouseDown += NotationLabel_MouseDown;
                    tempLabel.Name += item.LastMovePlayed.MoveNo.ToString();
                    tempLabel.Text += item.LastMovePlayed.ShortNotation;
                    tempLabel.Font = NotationFont;
                    tempLabel.AutoSize = true;
                    tempLabel.Tag = CurrentNode.Item1;
                    InsertLabelAnnotation(tempLabel, item.LastMovePlayed.Comment);
                    tempLabel.ForeColor = (CurrentNode.Item1 == MainLine ? NotationMainForeColor :
                        NotationSubForeColor);
                    if (CurrentPosition == item)
                    {
                        focusLabel = tempLabel;
                        tempLabel.ForeColor = NotationFocusColor;
                        CurrentVariation = CurrentNode.Item1;
                    }
                    flowLayoutPanel1.Controls.Add(tempLabel);
                    if (item.VariationHolders != null && item.VariationHolders.Count > 0)
                    {
                        int x = CurrentNode.Item1.MovesList.IndexOf(item) + 1;
                        tupleStack.Push(Tuple.Create(CurrentNode.Item1, x));
                        for (int i = item.VariationHolders.Count - 1; i >= 0; i--)
                        {
                            tupleStack.Push(Tuple.Create(item.VariationHolders[i], 0));
                        }
                        IsNewVar = true;
                        InsertNumbering = true;
                        break;
                    }
                }
                if (IsNewVar)
                    continue;
                if (CurrentNode.Item1 != MainLine)
                {
                    tempLabel = new Label();
                    tempLabel.Name = "Y";
                    tempLabel.Text = (CurrentNode.Item1.ParentLine == MainLine ? " ] " : ")");
                    tempLabel.Tag = CurrentNode.Item1;
                    flowLayoutPanel1.Controls.Add(tempLabel);
                    if (CurrentNode.Item1.ParentLine == MainLine)
                        breakList.Add(flowLayoutPanel1.Controls.Count - 1);
                    InsertNumbering = true;
                    tempLabel.ForeColor = NotationSubForeColor;
                    tempLabel.Font = NotationFont;
                    tempLabel.AutoSize = true;
                }
            }
            foreach (var item in breakList)
            {
                flowLayoutPanel1.SetFlowBreak(flowLayoutPanel1.Controls[item], true);
                flowLayoutPanel1.Controls[item].AccessibleName = "break";
            }
            flowLayoutPanel1.ResumeLayout();
            if (flowLayoutPanel1.VerticalScroll.Visible)
                flowLayoutPanel1.Size = ScrollSize2;
        }
        private void DeleteVariation(VariationHolder VarToDelete)
        {
            List<VariationHolder> tempList = 
                VarToDelete.ParentLine.MovesList[VarToDelete.ParentIndex + 1].VariationHolders;
            tempList.Remove(VarToDelete);
            if (tempList.Count == 0)
                tempList = null;
            if (CurrentVariation == VarToDelete || 
                CurrentVariation.ListOfParents.Contains(VarToDelete))
            {
                LoadPosition(VarToDelete.ParentLine.MovesList[VarToDelete.ParentIndex + 1], false);
                CurrentVariation = VarToDelete.ParentLine;     
            }
             
            List<VariationHolder> VarsToDelete = new List<VariationHolder>();
            foreach (var item in GameVariations)
                if (item.ListOfParents.Contains(VarToDelete))
                    VarsToDelete.Add(item);
            foreach (var item in VarsToDelete)
                GameVariations.Remove(item);

            ReInsertLabels();
            if (CurrentPosition != MainLine.MovesList[0])
                flowLayoutPanel1.ScrollControlIntoView(focusLabel);

            if (flowLayoutPanel1.VerticalScroll.Visible && flowLayoutPanel1.Size != ScrollSize2)
            {
                flowLayoutPanel1.Size = ScrollSize2;
            }
            else if (!flowLayoutPanel1.VerticalScroll.Visible && flowLayoutPanel1.Size != ScrollSize1)
            {
                flowLayoutPanel1.Size = ScrollSize1;
            }
        }
        private void LoadNextPosition()
        {
            Position TempPosition;
            index = CurrentVariation.MovesList.IndexOf(CurrentPosition);

            // VariationHolder list is contained in the actual Position object with the foregone variations

            if (index < CurrentVariation.MovesList.Count - 1)
            {
                if (CurrentVariation.MovesList[index + 1].VariationHolders != null && !quickLoad &&
                    CurrentVariation.MovesList[index + 1].VariationHolders.Count > 0)
                {
                    HandleVariationSelection(index);
                }
                else
                {
                    TempPosition = CurrentVariation.MovesList[index + 1];
                    LoadPosition(TempPosition, false);
                }

                if (focusLabel != null)
                {
                    if (focusLabel.Tag == MainLine)
                        focusLabel.ForeColor = NotationMainForeColor;
                    else
                        focusLabel.ForeColor = NotationSubForeColor;
                }

                foreach (var itemx in flowLayoutPanel1.Controls)
                {
                    Label item = itemx as Label;
                    
                    if (CurrentPosition.LastMovePlayed != null &&
                        item.Name.Substring(1, item.Name.Length - 1) == CurrentPosition.LastMovePlayed.MoveNo.ToString() &&
                        (VariationHolder)item.Tag == CurrentVariation)
                    {
                        if ((CurrentPosition.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White && item.Name[0] == 'W')
                            || CurrentPosition.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.Black && item.Name[0] == 'B')
                        {
                            focusLabel = item;
                            focusLabel.ForeColor = NotationFocusColor;
                            flowLayoutPanel1.Refresh();
                            break;
                        }
                    }
                }
            }
        }
        private void LoadPreviousPosition()
        {
            Position TempPostion;
            int x = CurrentVariation.MovesList.IndexOf(CurrentPosition);
            if (x > 0)
            {
                TempPostion = CurrentVariation.MovesList[x - 1];
                LoadPosition(TempPostion, false);
            }
            else
            {
                if (CurrentVariation.ParentLine != null)
                {
                    LoadPosition(CurrentVariation.ParentLine.MovesList[CurrentVariation.ParentIndex], false);
                    CurrentVariation = CurrentVariation.ParentLine;
                }
            }

            if (focusLabel != null)
            {
                if (focusLabel.Tag == MainLine)
                    focusLabel.ForeColor = NotationMainForeColor;
                else
                    focusLabel.ForeColor = NotationSubForeColor;
            }

            foreach (var itemx in flowLayoutPanel1.Controls)
            {
                Label item = itemx as Label;
                if (CurrentVariation == MainLine && CurrentPosition == CurrentVariation.MovesList[0])
                {
                    focusLabel = null;
                    break;
                }
                if (item.Name == "X" || item.Name == "Y")
                    continue;
                if (item.Name.Substring(1) == CurrentPosition.LastMovePlayed.MoveNo.ToString() &&
                    (VariationHolder)item.Tag == CurrentVariation)
                {
                    if ((CurrentPosition.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White && item.Name[0] == 'W')
                        || CurrentPosition.LastMovePlayed.PieceMoving.Side == Piece.PieceSide.Black && item.Name[0] == 'B')
                    {
                        focusLabel = item;
                        focusLabel.ForeColor = NotationFocusColor;
                        break;
                    }
                }
            }
        }
        private void HandleVariationSelection(int x)
        {
            focus = 0;
            Move tempMove = CurrentVariation.MovesList[x + 1].LastMovePlayed;
            Font font = new System.Drawing.Font("Segoe UI", 15F, FontStyle.Regular);
            VariationSelectForm = new Form();
            VariationSelectForm.StartPosition = FormStartPosition.Manual;
            VariationSelectForm.KeyDown += VariationSelectForm_KeyDown;
            VariationSelectForm.Text = "Select Variation";
            VariationSelectForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            VariationSelectForm.MaximizeBox = false;
            VariationSelectForm.MinimizeBox = false;
            VariationSelectForm.ShowIcon = false;
            VariationSelectForm.AutoScroll = true;
            VariationLabels = new List<Label>();
            Label tempLabel = new Label(), lastLabel = tempLabel;
            tempLabel.Text = tempMove.MoveNo.ToString();
            tempLabel.Font = font;
            tempLabel.MouseEnter += VariationLabel_MouseEnter;
            tempLabel.Text += tempMove.PieceMoving.Side == Piece.PieceSide.White
                ? " " : " . . . ";
            tempLabel.Text += tempMove.ShortNotation + "    Main line   ";
            tempLabel.Location = new Point(0, 0);
            tempLabel.AutoSize = true;
            VariationLabels.Add(tempLabel);
            VariationSelectForm.Controls.Add(tempLabel);
            foreach (var item in CurrentVariation.MovesList[x + 1].VariationHolders)
            {
                tempLabel = new Label();
                tempLabel.Text = item.ToString();
                tempLabel.AutoSize = true;
                tempLabel.Location = new Point(0, lastLabel.Bottom + 3);
                tempLabel.Font = font;
                tempLabel.MouseEnter += VariationLabel_MouseEnter;
                VariationLabels.Add(tempLabel);
                VariationSelectForm.Controls.Add(tempLabel);
                lastLabel = tempLabel;
            }
            VariationSelectForm.AutoScroll = true;
            foreach (var item in VariationLabels)
            {
                item.Click += VariationSelectionForm_Click;
                item.BackColor = System.Drawing.Color.White;
                item.ForeColor = System.Drawing.Color.Black;
            }
            VariationLabels[focus].BackColor = Color.DeepSkyBlue;
            VariationLabels[focus].ForeColor = System.Drawing.Color.White;

            VariationSelectForm.Height = 350;
            Point location = new Point();
            location = Cursor.Position -
                new Size(VariationSelectForm.Width / 2, VariationSelectForm.Height / 2);
            if (location.X < this.Location.X)
                location.X = this.Location.X + 5;
            if (location.Y < this.Location.Y)
                location.Y = this.Location.Y + 5;
            if (location.X + VariationSelectForm.Width > this.Right)
                location.X = this.Right - VariationSelectForm.Width - 5;
            if (location.Y + VariationSelectForm.Height > this.Bottom)
                location.Y = this.Bottom - VariationSelectForm.Height - 5;
            VariationSelectForm.Location = location;
            VariationSelectForm.ShowDialog(this);
        }
        void VariationLabel_MouseEnter(object sender, EventArgs e)
        {
            foreach (var item in VariationLabels)
            {
                item.BackColor = System.Drawing.Color.White;
                item.ForeColor = System.Drawing.Color.Black;
            }
            (sender as Label).ForeColor = Color.White;
            (sender as Label).BackColor = Color.DeepSkyBlue;
            focus = VariationLabels.IndexOf(sender as Label);
        }
        void VariationSelectForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Enter)
            {
                if (focus == 0)
                    LoadPosition(CurrentVariation.MovesList[index + 1], false);
                else
                {
                    CurrentVariation = CurrentVariation.MovesList[index + 1].VariationHolders[focus - 1];
                    LoadPosition(CurrentVariation.MovesList[0], false);
                }
                VariationSelectForm.Close();    
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (focus == 0)
                    focus = VariationLabels.Count - 1;
                else
                    focus--;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (focus == VariationLabels.Count - 1)
                    focus = 0;
                else
                    focus++;
            }
            else if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Left)
            {
                VariationSelectForm.Close();
            }
            foreach (var item in VariationLabels)
            {
                item.BackColor = System.Drawing.Color.White;
                item.ForeColor = System.Drawing.Color.Black;
            }
            VariationLabels[focus].BackColor = System.Drawing.Color.DeepSkyBlue;
            VariationLabels[focus].ForeColor = System.Drawing.Color.White;
            if (VariationSelectForm.VerticalScroll.Enabled)
                VariationSelectForm.ScrollControlIntoView(VariationLabels[focus]);
            VariationSelectForm.Refresh();
        }
        void VariationSelectionForm_Click(object sender, EventArgs e)
        {
            Label tempLabel = sender as Label;
            focus = VariationLabels.IndexOf(tempLabel);

            if (focus == 0)
                LoadPosition(CurrentVariation.MovesList[index + 1], false);
            else
            {
                CurrentVariation = CurrentVariation.MovesList[index + 1].VariationHolders[focus - 1];
                LoadPosition(CurrentVariation.MovesList[0], false);
            }
            VariationSelectForm.Close();
        }
        private void LoadPosition(Position TempPosition, bool isPlayOut)
        {
            if (!isPlayOut)
            {
                if (IsAnimating)
                    return;
                AnimationList = new List<AnimationTask>();
                foreach (var item in LoadedEngines)
                {
                    item.Process.StandardInput.WriteLine("stop");
                }
                if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0 && !isInfiniteSearch)
                {
                    List<Square> list = new List<Square>();
                    foreach (var item in CurrentPosition.Lines)
                        list.AddRange(item.Squares);
                    RefreshSquares(list, RedrawPerspective.UserArrows, null);
                }
                UpdateArrow(true);
                if (TempPosition != CurrentPosition)
                    BestMoveArrow.IsInvalid = true;
                if (HighlightedSquares != null)
                {
                    foreach (var item in HighlightedSquares)
                    {
                        using (Graphics g = panel1.CreateGraphics())
                        {
                            g.FillRectangle(item.Type == Square.SquareType.Light ?
                                LightSquareColor : DarkSquareColor, item.Rectangle);
                            if (item.Piece != null)
                                PlacePiece(item.Piece, item);
                        }
                    }
                    isHighlighted = false;
                    HighlightedSquares.Clear();
                }
            }

            CheckingPiece = TempPosition.CheckingPiece;
            DoubleCheckingPiece = TempPosition.DoubleCheckingPiece;
            IsBlackCheckmated = TempPosition.IsBlackCheckmated;
            IsBlackInCheck = TempPosition.IsBlackInCheck;
            IsDraw = TempPosition.IsDraw;
            IsWhiteCheckmated = TempPosition.IsWhiteCheckmated;
            IsWhiteInCheck = TempPosition.IsWhiteInCheck;
            KingsideCastlingBlack = TempPosition.KingsideCastlingBlack;
            KingsideCastlingWhite = TempPosition.KingsideCastlingWhite;
            QueensideCastlingBlack = TempPosition.QueensideCastlingBlack;
            QueensideCastlingWhite = TempPosition.QueensideCastlingWhite;
            EnPassantPawn = TempPosition.EnPassantPawn;
            sideToPlay = TempPosition.sideToPlay;
            FiftyMoveCount = TempPosition.FiftyMoveCount;
            PossibleDefences = TempPosition.PossibleDefences;
            MoveCount = TempPosition.MoveCount;
            Pieces.AddRange(CapturedPieces);
            CapturedPieces = new List<Piece>();

            bool OKAY = false;
            foreach (var item in Pieces)
            {
                foreach (var item2 in TempPosition.PieceInfos)
                {
                    if (item2.Piece == item)
                    { OKAY = true; continue; }
                }
                if (!OKAY)
                {
                    CapturedPieces.Add(item);
                }
                OKAY = false;
            }
            foreach (var item in CapturedPieces)
            {
                Pieces.Remove(item);

                if (item.Square != null)
                {
                    if (!isPlayOut)
                        using (Graphics g = panel1.CreateGraphics())
                            g.FillRectangle(item.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                            LightSquareColor, item.Square.Rectangle);
                    if (item.Square.Piece != null)
                        item.Square.Piece = null;
                    item.Square = null;
                }
            }

            foreach (var item in TempPosition.PieceInfos)
            {
                Square tempSquare = item.Square;
                if (item.Piece.Square != tempSquare)
                {
                    if (item.Piece.Square == null)
                    {
                        if (tempSquare.Piece != null)
                        {
                            tempSquare.Piece.Square = null;
                            if (!isPlayOut)
                                using (Graphics g = panel1.CreateGraphics())
                                    g.FillRectangle(tempSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                                    LightSquareColor, tempSquare.Rectangle);
                        }
                        if (!ShouldAnimate || isPlayOut ||
                            item.Piece.Square == null || item.Piece.Square == tempSquare)
                            PlacePiece(item.Piece, tempSquare, isPlayOut);
                        else
                        {
                            IsAnimating = true;
                            AnimationList.Add(new AnimationTask
                                (new Move(item.Piece.Square, item.Piece, tempSquare)));
                            if (item.Piece.Square != null && item.Piece.Square.Piece != null)
                                item.Piece.Square.Piece = null;
                            item.Piece.Square = tempSquare;
                            tempSquare.Piece = item.Piece;
                        }
                    }
                    else
                    {
                        item.Piece.Square.Piece = null;
                        if (!isPlayOut)
                            using (Graphics g = panel1.CreateGraphics())
                                g.FillRectangle(item.Piece.Square.Type == Square.SquareType.Dark ? DarkSquareColor :
                                LightSquareColor, item.Piece.Square.Rectangle);
                        if (tempSquare.Piece != null)
                        {
                            tempSquare.Piece.Square = null;
                            if (!isPlayOut)
                                using (Graphics g = panel1.CreateGraphics())
                                    g.FillRectangle(tempSquare.Type == Square.SquareType.Dark ? DarkSquareColor :
                                    LightSquareColor, tempSquare.Rectangle);
                        }
                        if (!ShouldAnimate || isPlayOut ||
                            item.Piece.Square == null || item.Piece.Square == tempSquare)
                            PlacePiece(item.Piece, tempSquare, isPlayOut);
                        else
                        {
                            IsAnimating = true;
                            AnimationList.Add(new AnimationTask
                                (new Move(item.Piece.Square, item.Piece, tempSquare)));
                            if (item.Piece.Square != null && item.Piece.Square.Piece != null)
                                item.Piece.Square.Piece = null;
                            item.Piece.Square = tempSquare;
                            tempSquare.Piece = item.Piece;
                        }
                    }
                }
            }

            CurrentPosition = TempPosition;

            if (!isPlayOut)
            {
                ShowOpening();
                if (!IsAnimating && LastMoveHighlighted != CurrentPosition.LastMovePlayed)
                    HighLightLastMove(CurrentPosition.LastMovePlayed);
                if (!IsAnimating)
                    HighlightCheckedKing();
                if (!IsAnimating && ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0
                    && !isInfiniteSearch && !BestMoveArrow.Enabled)
                {
                    List<Square> list = new List<Square>();
                    foreach (var item in CurrentPosition.Lines)
                    {
                        item.Enabled = true;
                        list.AddRange(item.Squares);
                    }
                    RefreshSquares(list, RedrawPerspective.UserArrows, null);
                }   

                AnimationTimer.Start();

                foreach (var item in LoadedEngines)
                {
                    SendPositionToEngine(item, "");
                    if (item.isAnalyzing)
                    {
                        item.Process.StandardInput.WriteLine("go infinite");
                        item.ShouldIgnore = false;
                    }
                }

                if (!IsAnimating && isInfiniteSearch)
                {
                    ArrowTimer.Enabled = true;
                    ArrowTimer.Start();
                }

                SSCurrentLabel = null;
                int index = MainLine.MovesList.IndexOf(CurrentPosition);
                if (index > 0)
                {
                    List<Label> list = SSPanel.Tag as List<Label>;
                    SSCurrentLabel = list[index - 1];
                }
                if (SSPanel.Tag != null)
                {
                    bool PASSED = false;
                    if (CurrentVariation != MainLine)
                        SSCurrentLabel = null;
                    List<Label> list = SSPanel.Tag as List<Label>;
                    list.Reverse();
                    foreach (var item in list)
                    {
                        if (PASSED || item == SSCurrentLabel)
                        {
                            item.ForeColor = Color.Black;
                            PASSED = true;
                        }
                        else
                        {
                            item.ForeColor = Color.DimGray;
                        }
                    }
                    list.Reverse();
                }
                if (SSCurrentLabel != null)
                    SSPanel.ScrollControlIntoView(SSCurrentLabel);
            }

            if (isInfiniteSearch)
            {
                if (LoadedEngines != null)
                    foreach (var item in LoadedEngines)
                    {
                        if (!item.isUciEngine)
                            continue;
                        item.AnalysisTime = TimeSpan.Zero;
                    }
            }
        }
        private void UpdateFocusLabel()
        {
            if (focusLabel != null)
            {
                focusLabel.Font = NotationFont;
                if (focusLabel.Tag == MainLine)
                    focusLabel.ForeColor = NotationMainForeColor;
                else
                    focusLabel.ForeColor = NotationSubForeColor;
                focusLabel = null;
            }
            if (CurrentPosition.LastMovePlayed != null)
                foreach (var item in flowLayoutPanel1.Controls)
                {    
                    Label label = item as Label;
                    if (label.Tag == CurrentVariation &&
                        label.Name.Substring(1) == CurrentPosition.LastMovePlayed.MoveNo.ToString())
                    {
                        if ((CurrentPosition.LastMovePlayed.PieceMoving.Side
                            == Piece.PieceSide.White && label.Name[0] == 'W')
                        || CurrentPosition.LastMovePlayed.PieceMoving.Side
                        == Piece.PieceSide.Black && label.Name[0] == 'B')
                        {
                            focusLabel = label;
                            focusLabel.ForeColor = NotationFocusColor;
                            flowLayoutPanel1.ScrollControlIntoView(focusLabel);
                            break;
                        }
                    }
                }
        }
        private List<PieceInfo> ClonePieces()
        {
            List<PieceInfo> tempList = new List<PieceInfo>();
            foreach (var item in Squares)
            {
                if (item.Piece == null)
                    continue;
                PieceInfo temp = new PieceInfo(item.Piece, item);
                tempList.Add(temp);
            }
            return tempList;
        }
        [Serializable]
        public class Position
        {
            public Position()
            {
                Lines = new List<Arrow>();
            }
            public override string ToString()
            {       
                return (LastMovePlayed != null ? LastMovePlayed.ToString() : "Startpos");
            }
            public bool Compare(object obj)
            {
                if (!(obj is Position || obj is List<PieceInfo> || 
                    obj is OpeningNode))
                    return false;
                if (obj is OpeningNode)
                {
                    var TupleData = (obj as OpeningNode).StringTuple;
                    for (int i = 0; i < PieceInfos.Count; i++)
                    {
                        if (PieceInfos[i].Piece.Name != TupleData[i].Item1 ||
                            PieceInfos[i].Square.Name != TupleData[i].Item2)
                            return false;
                    }
                    return this.sideToPlay == (obj as OpeningNode).SideToPlay;
                }

                List<PieceInfo> other;
                if (obj is List<PieceInfo>)
                    other = obj as List<PieceInfo>;
                else
                    other = (obj as Position).PieceInfos;
                if (this.PieceInfos.Count != other.Count)
                    return false;

                for (int i = 0; i < this.PieceInfos.Count; i++)
                {
                    if (!PieceInfos[i].Piece.Compare(other[i].Piece)
                        || !PieceInfos[i].Square.Compare(other[i].Square))
                        return false;
                }
                return true;
            }
            public List<VariationHolder> VariationHolders { get; set; }
            public List<PieceInfo> PieceInfos { get; set; }
            public Move LastMovePlayed { get; set; }
            public bool IsWhiteInCheck { get; set; }
            public bool IsBlackInCheck { get; set; }
            public Piece.PieceSide sideToPlay { get; set; }
            public List<Arrow> Lines { get; set; }
            public bool KingsideCastlingWhite { get; set; }
            public bool QueensideCastlingWhite { get; set; }
            public bool KingsideCastlingBlack { get; set; }
            public bool QueensideCastlingBlack { get; set; }
            public Piece CheckingPiece { get; set; }
            public Piece DoubleCheckingPiece { get; set; }
            public List<Move> PossibleDefences { get; set; }
            public bool IsWhiteCheckmated { get; set; }
            public bool IsBlackCheckmated { get; set; }
            public bool IsDraw { get; set; }
            public int FiftyMoveCount { get; set; }
            public int MoveCount { get; set; }
            public Piece EnPassantPawn { get; set; }
        }
        [Serializable]
        public class PieceInfo: IEquatable<PieceInfo>
        {
            public PieceInfo()
            {

            }
            public PieceInfo(Piece piece, Square square)
            {
                Piece = piece;
                Square = square;
            }
            public bool Equals(PieceInfo other)
            {
                return Equals((object)other);
            }
            public override bool Equals(object obj)
            {
                if (obj is PieceInfo)
                    return this == obj;
                if (obj is Piece)
                    return this.Piece == obj;
                else
                    return false;
            }
            public override string ToString()
            {
                return Piece.ToString();
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public Piece Piece { get; set; }
            public Square Square { get; set; }
        }
        [Serializable]
        public class VariationHolder
        {
            public VariationHolder()
            {
                ListOfParents = new List<VariationHolder>();
            }
            public VariationHolder(List<Position> movesList)
            {
                MovesList = movesList;
                ListOfParents = new List<VariationHolder>();
            }
            public VariationHolder(List<Position> movesList, VariationHolder parentLine)
            {
                MovesList = movesList;
                ParentLine = parentLine;
                ListOfParents = new List<VariationHolder>();
            }
            public override string ToString()
            {
                String tempStr = "";
                if (MovesList[0].LastMovePlayed != null)
                {
                    if (MovesList[0].LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White)
                        tempStr += MovesList[0].LastMovePlayed.MoveNo.ToString() + ". ";
                    else
                        tempStr += MovesList[0].LastMovePlayed.MoveNo.ToString() + " . . . ";
                    tempStr += MovesList[0].LastMovePlayed.ShortNotation + " ";

                }
                for (int i = 1; i < 4; i++)
                {
                    if (MovesList.Count == i)
                        break;
                    if (MovesList[i].LastMovePlayed != null)
                    {
                        if (MovesList[i].LastMovePlayed.PieceMoving.Side == Piece.PieceSide.White)
                            tempStr += MovesList[i].LastMovePlayed.MoveNo.ToString() + ". ";

                        tempStr += MovesList[i].LastMovePlayed.ShortNotation + " ";

                    }
                }

                return tempStr;
            }
            public VariationHolder ParentLine { get; set; }
            public List<Position> MovesList { get; set; }
            public List<VariationHolder> ListOfParents { get; set; }
            public int ParentIndex { get; set; }
        }
        [Serializable]
        public class Comment
        {
            public Comment ()
            {
                LongComment = "";
                MoveComment = Form1.MoveComment.None;
                PositionComment = Form1.PositionComment.None;
            }
            public String LongComment { get; set; }
            public MoveComment MoveComment { get; set; }
            public PositionComment PositionComment { get; set; }
        }
        public enum PositionComment
        {
            None,
            EqualPosition,
            SlightWhiteAdv,
            SlightBlackAdv,
            ModerateWhiteAdv,
            ModerateBlackAdv,
            DecisiveWhiteAdv,
            DecisiveBlackAdv
        }
        public enum MoveComment
        {
            None,
            InterestingMove,
            DubiousMove,
            StrongMove,
            WeakMove,
            GreatMove,
            TerribleMove
        }
    }
}
