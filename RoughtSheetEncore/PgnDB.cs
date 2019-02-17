using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace RoughtSheetEncore
{
    partial class Form1: Form
    {

        #region Declarations
        public enum SortLogic
        {
            White,
            Black,
            Result,
            Date,
            Event,
            Round,
            ECO
        }
        public enum SortOrder
        {
            Ascending,
            Descending
        }

        private Panel DBEPanel;
        private Label DBEWhiteLabel;
        private Label DBEDateLabel;
        private Label DBEECOLabel;
        private Label DBEBlackLabel;
        private Label DBEResultLabel;
        private Label DBEEventLabel;
        private Label Split1;
        private Label Split2;
        private Label Split5;
        private Label Split3;
        private Label Split4;
        private Label Split6;
        DatabaseExplorer DBExplorer = new DatabaseExplorer();
        String ClipBoardBuffer;
        List<Column> DBColumns = new List<Column>();
        Label SplitLabelClicked;
        Point SplitLocation;
        RecordLabelSet RecordSelected;
        #endregion

        private void InitializeDBEForm()
        {
            this.DBEPanel = new System.Windows.Forms.Panel();
            this.Split1 = new System.Windows.Forms.Label();
            this.DBEDateLabel = new System.Windows.Forms.Label();
            this.DBEECOLabel = new System.Windows.Forms.Label();
            this.DBEBlackLabel = new System.Windows.Forms.Label();
            this.DBEResultLabel = new System.Windows.Forms.Label();
            this.DBEEventLabel = new System.Windows.Forms.Label();
            this.DBEWhiteLabel = new System.Windows.Forms.Label();
            this.Split4 = new System.Windows.Forms.Label();
            this.Split6 = new System.Windows.Forms.Label();
            this.Split3 = new System.Windows.Forms.Label();
            this.Split5 = new System.Windows.Forms.Label();
            this.Split2 = new System.Windows.Forms.Label();
            this.DBEPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DBEPanel
            // 
            this.DBEPanel.BackColor = System.Drawing.Color.White;
            this.DBEPanel.Controls.Add(this.Split2);
            this.DBEPanel.Controls.Add(this.Split6);
            this.DBEPanel.Controls.Add(this.Split5);
            this.DBEPanel.Controls.Add(this.Split3);
            this.DBEPanel.Controls.Add(this.Split4);
            this.DBEPanel.Controls.Add(this.Split1);
            this.DBEPanel.Controls.Add(this.DBEDateLabel);
            this.DBEPanel.Controls.Add(this.DBEECOLabel);
            this.DBEPanel.Controls.Add(this.DBEBlackLabel);
            this.DBEPanel.Controls.Add(this.DBEResultLabel);
            this.DBEPanel.Controls.Add(this.DBEEventLabel);
            this.DBEPanel.Controls.Add(this.DBEWhiteLabel);
            this.DBEPanel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.DBEPanel.Location = new System.Drawing.Point(20, 69);
            this.DBEPanel.Margin = new System.Windows.Forms.Padding(0);
            this.DBEPanel.Name = "DBEPanel";
            this.DBEPanel.Size = new System.Drawing.Size(815, 420);
            this.DBEPanel.TabIndex = 0;
            DBEPanel.AutoScroll = true;
            // 
            // DBEDateLabel
            // 
            this.DBEDateLabel.BackColor = System.Drawing.Color.White;
            this.DBEDateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBEDateLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DBEDateLabel.Location = new System.Drawing.Point(578, 0);
            this.DBEDateLabel.Name = "DBEDateLabel";
            this.DBEDateLabel.Size = new System.Drawing.Size(70, 27);
            this.DBEDateLabel.TabIndex = 5;
            this.DBEDateLabel.Text = "Date";
            this.DBEDateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            DBEDateLabel.MouseEnter += DBETitleLabel_MouseEnter;
            DBEDateLabel.MouseLeave += DBETitleLabel_MouseLeave;
            Column DateColumn = new Column();
            DateColumn.Name = DBEDateLabel.Text;
            DateColumn.TitleLabel = DBEDateLabel;
            DateColumn.Width = DBEDateLabel.Width;
            DBEDateLabel.Tag = DateColumn;
            DBColumns.Add(DateColumn);
            // 
            // DBEECOLabel
            // 
            this.DBEECOLabel.BackColor = System.Drawing.Color.White;
            this.DBEECOLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBEECOLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DBEECOLabel.Location = new System.Drawing.Point(678, 0);
            this.DBEECOLabel.Name = "DBEECOLabel";
            this.DBEECOLabel.Size = new System.Drawing.Size(50, 27);
            this.DBEECOLabel.TabIndex = 4;
            this.DBEECOLabel.Text = "ECO";
            this.DBEECOLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            DBEECOLabel.MouseEnter += DBETitleLabel_MouseEnter;
            DBEECOLabel.MouseLeave += DBETitleLabel_MouseLeave;
            Column ECOColumn = new Column();
            ECOColumn.Name = DBEECOLabel.Text;
            ECOColumn.TitleLabel = DBEECOLabel;
            ECOColumn.Width = DBEECOLabel.Width;
            DBEECOLabel.Tag = ECOColumn;
            DBColumns.Add(ECOColumn);
            // 
            // DBEBlackLabel
            // 
            this.DBEBlackLabel.BackColor = System.Drawing.Color.White;
            this.DBEBlackLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBEBlackLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DBEBlackLabel.Location = new System.Drawing.Point(168, 0);
            this.DBEBlackLabel.Name = "DBEBlackLabel";
            this.DBEBlackLabel.Size = new System.Drawing.Size(169, 27);
            this.DBEBlackLabel.TabIndex = 3;
            this.DBEBlackLabel.Text = "Black";
            this.DBEBlackLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            DBEBlackLabel.MouseEnter += DBETitleLabel_MouseEnter;
            DBEBlackLabel.MouseLeave += DBETitleLabel_MouseLeave;
            Column BlackColumn = new Column();
            BlackColumn.Name = DBEBlackLabel.Text;
            BlackColumn.TitleLabel = DBEBlackLabel;
            BlackColumn.Width = DBEBlackLabel.Width;
            DBEBlackLabel.Tag = BlackColumn;
            DBColumns.Add(BlackColumn);
            // 
            // DBEResultLabel
            // 
            this.DBEResultLabel.BackColor = System.Drawing.Color.White;
            this.DBEResultLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBEResultLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DBEResultLabel.Location = new System.Drawing.Point(333, 0);
            this.DBEResultLabel.Name = "DBEResultLabel";
            this.DBEResultLabel.Size = new System.Drawing.Size(50, 27);
            this.DBEResultLabel.TabIndex = 2;
            this.DBEResultLabel.Text = "Result";
            this.DBEResultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            DBEResultLabel.MouseEnter += DBETitleLabel_MouseEnter;
            DBEResultLabel.MouseLeave += DBETitleLabel_MouseLeave;
            Column ResultColumn = new Column();
            ResultColumn.Name = DBEResultLabel.Text;
            ResultColumn.TitleLabel = DBEResultLabel;
            ResultColumn.Width = DBEResultLabel.Width;
            DBEResultLabel.Tag = ResultColumn;
            DBColumns.Add(ResultColumn);
            // 
            // DBEEventLabel
            // 
            this.DBEEventLabel.BackColor = System.Drawing.Color.White;
            this.DBEEventLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBEEventLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DBEEventLabel.Location = new System.Drawing.Point(403, 0);
            this.DBEEventLabel.Name = "DBEEventLabel";
            this.DBEEventLabel.Size = new System.Drawing.Size(145, 27);
            this.DBEEventLabel.TabIndex = 1;
            this.DBEEventLabel.Text = "Event";
            this.DBEEventLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            DBEEventLabel.MouseEnter += DBETitleLabel_MouseEnter;
            DBEEventLabel.MouseLeave += DBETitleLabel_MouseLeave;
            Column EventColumn = new Column();
            EventColumn.Name = DBEEventLabel.Text;
            EventColumn.TitleLabel = DBEEventLabel;
            EventColumn.Width = DBEEventLabel.Width;
            DBEEventLabel.Tag = EventColumn;
            DBColumns.Add(EventColumn);
            // 
            // DBEWhiteLabel
            // 
            this.DBEWhiteLabel.BackColor = System.Drawing.Color.White;
            this.DBEWhiteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DBEWhiteLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.DBEWhiteLabel.Location = new System.Drawing.Point(10, 10);
            this.DBEWhiteLabel.Name = "DBEWhiteLabel";
            this.DBEWhiteLabel.Size = new System.Drawing.Size(168, 27);
            this.DBEWhiteLabel.TabIndex = 0;
            this.DBEWhiteLabel.Text = "White";
            this.DBEWhiteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            DBEWhiteLabel.MouseEnter += DBETitleLabel_MouseEnter;
            DBEWhiteLabel.MouseLeave += DBETitleLabel_MouseLeave;
            Column WhiteColumn = new Column();
            WhiteColumn.Name = DBEWhiteLabel.Text;
            WhiteColumn.TitleLabel = DBEWhiteLabel;
            WhiteColumn.Width = DBEWhiteLabel.Width;
            DBEWhiteLabel.Tag = WhiteColumn;
            DBColumns.Add(WhiteColumn);

            WhiteColumn.NextColumn = BlackColumn;
            BlackColumn.PreviousColumn = WhiteColumn;
            BlackColumn.NextColumn = ResultColumn;
            ResultColumn.PreviousColumn = BlackColumn;
            ResultColumn.NextColumn = EventColumn;
            EventColumn.PreviousColumn = ResultColumn;
            EventColumn.NextColumn = DateColumn;
            DateColumn.PreviousColumn = EventColumn;
            DateColumn.NextColumn = ECOColumn;
            ECOColumn.PreviousColumn = DateColumn;
            // 
            // Split1
            // 
            this.Split1.BackColor = System.Drawing.Color.White;
            this.Split1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Split1.ForeColor = System.Drawing.SystemColors.Control;
            this.Split1.Location = new System.Drawing.Point(167, 5);
            this.Split1.Name = "Split1";
            Split1.Text = "|";
            this.Split1.Size = new System.Drawing.Size(10, 25);
            this.Split1.TabIndex = 6;
            this.Split1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Split4
            // 
            this.Split4.BackColor = System.Drawing.Color.White;
            this.Split4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Split4.ForeColor = System.Drawing.SystemColors.Control;
            this.Split4.Location = new System.Drawing.Point(384, 198);
            this.Split4.Name = "Split4";
            this.Split4.Size = new System.Drawing.Size(10, 25);
            this.Split4.TabIndex = 7;
            this.Split4.Text = "|";
            this.Split4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Split3
            // 
            this.Split3.BackColor = System.Drawing.Color.White;
            this.Split3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Split3.ForeColor = System.Drawing.SystemColors.Control;
            this.Split3.Location = new System.Drawing.Point(309, 54);
            this.Split3.Name = "Split3";
            this.Split3.Size = new System.Drawing.Size(10, 25);
            this.Split3.TabIndex = 8;
            this.Split3.Text = "|";
            this.Split3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Split5
            // 
            this.Split5.BackColor = System.Drawing.Color.White;
            this.Split5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Split5.ForeColor = System.Drawing.SystemColors.Control;
            this.Split5.Location = new System.Drawing.Point(400, 214);
            this.Split5.Name = "Split5";
            this.Split5.Size = new System.Drawing.Size(10, 25);
            this.Split5.TabIndex = 9;
            this.Split5.Text = "|";
            this.Split5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Split6
            // 
            this.Split6.BackColor = System.Drawing.Color.White;
            this.Split6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Split6.ForeColor = System.Drawing.SystemColors.Control;
            this.Split6.Location = new System.Drawing.Point(400, 214);
            this.Split6.Name = "Split6";
            this.Split6.Size = new System.Drawing.Size(10, 25);
            this.Split6.TabIndex = 9;
            this.Split6.Text = "|";
            this.Split6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Split2
            // 
            this.Split2.BackColor = System.Drawing.Color.White;
            this.Split2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Split2.ForeColor = System.Drawing.SystemColors.Control;
            this.Split2.Location = new System.Drawing.Point(408, 222);
            this.Split2.Name = "Split2";
            this.Split2.Size = new System.Drawing.Size(10, 25);
            this.Split2.TabIndex = 10;
            this.Split2.Text = "|";
            this.Split2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            Split1.Tag = Tuple.Create(WhiteColumn, BlackColumn);
            Split2.Tag = Tuple.Create(BlackColumn, ResultColumn);
            Split3.Tag = Tuple.Create(ResultColumn, EventColumn);
            Split4.Tag = Tuple.Create(EventColumn, DateColumn);
            Split5.Tag = Tuple.Create(DateColumn, ECOColumn);
            Split6.Tag = Tuple.Create(ECOColumn);
            WhiteColumn.NextSplit = Split1;
            BlackColumn.NextSplit = Split2;
            ResultColumn.NextSplit = Split3;
            EventColumn.NextSplit = Split4;
            DateColumn.NextSplit = Split5;
            ECOColumn.NextSplit = Split6;
            Split1.Cursor = Cursors.VSplit;
            Split2.Cursor = Cursors.VSplit;
            Split3.Cursor = Cursors.VSplit;
            Split4.Cursor = Cursors.VSplit;
            Split5.Cursor = Cursors.VSplit;
            Split6.Cursor = Cursors.VSplit;
            Split1.MouseDown += Split_MouseDown;
            Split2.MouseDown += Split_MouseDown;
            Split3.MouseDown += Split_MouseDown;
            Split4.MouseDown += Split_MouseDown;
            Split5.MouseDown += Split_MouseDown;
            Split6.MouseDown += Split_MouseDown;
            Split1.MouseUp += Split_MouseUp;
            Split2.MouseUp += Split_MouseUp;
            Split3.MouseUp += Split_MouseUp;
            Split4.MouseUp += Split_MouseUp;
            Split5.MouseUp += Split_MouseUp;
            Split6.MouseUp += Split_MouseUp;
            Split1.MouseMove += Split_MouseMove;
            Split2.MouseMove += Split_MouseMove;
            Split3.MouseMove += Split_MouseMove;
            Split4.MouseMove += Split_MouseMove;
            Split5.MouseMove += Split_MouseMove;
            Split6.MouseMove += Split_MouseMove;

            Split1.Location = DBEWhiteLabel.Location + new Size(DBEWhiteLabel.Width + 3, 0);
            DBEBlackLabel.Location = Split1.Location + new Size(10, 0);
            Split2.Location = DBEBlackLabel.Location + new Size(DBEBlackLabel.Width + 3, 0);
            DBEResultLabel.Location = Split2.Location + new Size(10, 0);
            Split3.Location = DBEResultLabel.Location + new Size(DBEResultLabel.Width + 3, 0);
            DBEEventLabel.Location = Split3.Location + new Size(10, 0);
            Split4.Location = DBEEventLabel.Location + new Size(DBEEventLabel.Width + 3, 0);
            DBEDateLabel.Location = Split4.Location + new Size(10, 0);
            Split5.Location = DBEDateLabel.Location + new Size(DBEDateLabel.Width + 3, 0);
            DBEECOLabel.Location = Split5.Location + new Size(10, 0);
            Split6.Location = DBEECOLabel.Location + new Size(DBEECOLabel.Width + 3, 0);
            // 
            // Form1
            // 
            DBExplorer.Form.ClientSize = new System.Drawing.Size(863, 513);
            DBExplorer.Form.Controls.Add(DBEPanel);
            DBExplorer.Form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            DBExplorer.Form.BackColor = Color.Gray;
            DBExplorer.Form.Name = "Form1";
            DBEPanel.ResumeLayout(false);
            DBExplorer.Form.ResumeLayout(false);
        }
        void DBERecordLabel_MouseLeave(object sender, EventArgs e)
        {
            Label label = sender as Label;
            RecordLabelSet rls = label.Tag as RecordLabelSet;
            if (!rls.IsHighlighted)
                foreach (var item in rls.Labels)
                    item.Item1.BackColor = Color.White;
        }
        void DBERecordLabel_MouseEnter(object sender, EventArgs e)
        {
            Label label = sender as Label;
            RecordLabelSet rls = label.Tag as RecordLabelSet;
            if (!rls.IsHighlighted)
            foreach (var item in rls.Labels)
                item.Item1.BackColor = Color.AliceBlue;
            
        }
        private void DBERecordLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;   // Change Later
            Label label = sender as Label;
            RecordLabelSet rls = label.Tag as RecordLabelSet;
            if (RecordSelected != null)
            {
                foreach (var item in RecordSelected.Labels)
                    item.Item1.BackColor = Color.White;
                RecordSelected.IsHighlighted = false;
            }
            RecordSelected = rls;
            foreach (var item in RecordSelected.Labels)
                item.Item1.BackColor = Color.PowderBlue;
            RecordSelected.IsHighlighted = true;
        }
        void DBETitleLabel_MouseEnter(object sender, EventArgs e)
        {
            Label label = sender as Label;
            label.BackColor = Color.PowderBlue;
        }
        void DBETitleLabel_MouseLeave(object sender, EventArgs e)
        {
            Label label = sender as Label;
            label.BackColor = Color.White;
        }
        void UpdateSplitResize(Label splitLabel, int p)
        {
            if (p == 0)
                return;
            if (splitLabel.Tag is Tuple<Column>)
            {
                return;
            }
            var tupleData = splitLabel.Tag as Tuple<Column, Column>;
            tupleData.Item1.TitleLabel.Width -= p;
            tupleData.Item1.Width -= p;
            //foreach (var item in DBExplorer.Games)
            //{
            //    RecordLabelSet rls = item.RecordLabelSet;
            //    foreach (var column in rls.Labels)
            //        if (column.Item2 == tupleData.Item1)
            //            column.Item1.Width -= p;
            //}
            Label Current = tupleData.Item2.TitleLabel;
            while (true)
            {
                Current.Location -= new Size(p, 0);
                if (Current.Tag is Column)
                {
                    Column temp = Current.Tag as Column;
                    foreach (var item in DBExplorer.Games)
                    //{
                    //    RecordLabelSet rls = item.RecordLabelSet;
                    //    foreach (var column in rls.Labels)
                    //        if (column.Item2 == temp)
                    //            column.Item1.Location -= new Size(p, 0);
                    //}
                    Current = temp.NextSplit;
                }
                else if (Current.Tag is Tuple<Column, Column>)
                {
                    var temp = Current.Tag as Tuple<Column, Column>;
                    Current = temp.Item2.TitleLabel;
                }
                else if (Current.Tag is Tuple<Column>)
                {
                    break;
                }
            }
            foreach (var item in DBExplorer.Games)
            {
                //RecordLabelSet rls = item.RecordLabelSet;
                
            }
        }
        void DisplayDBRecords(List<Game> TempList)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            DBEPanel.SuspendLayout();
            //if (TempList[0].RecordLabelSet == null)
                foreach (var item in TempList)
                {
                    //item.RecordLabelSet = new RecordLabelSet(item);
                    foreach (var column in DBColumns)
                    {
                        Label tempLabel = new Label();
                        tempLabel.AutoSize = false;
                        tempLabel.BackColor = Color.White;
                        tempLabel.ForeColor = Color.Black;
                        tempLabel.TextAlign = ContentAlignment.MiddleLeft;
                        tempLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, 
                            System.Drawing.FontStyle.Regular);
                        tempLabel.MouseEnter += DBERecordLabel_MouseEnter;
                        tempLabel.MouseLeave += DBERecordLabel_MouseLeave;
                        tempLabel.AutoEllipsis = true;
                        tempLabel.MouseDown += DBERecordLabel_MouseDown;
                        //tempLabel.Tag = item.RecordLabelSet;
                        switch (column.Name)
                        {
                            case "White":
                                tempLabel.Text = item.GameDetails.WhitePlayer.Name;
                                break;
                            case "Black":
                                tempLabel.Text = item.GameDetails.BlackPlayer.Name;
                                break;
                            case "Event":
                                tempLabel.Text = item.GameDetails.Event;
                                break;
                            case "ECO":
                                tempLabel.Text = item.GameDetails.ECO;
                                break;
                            case "Date":
                                tempLabel.Text = item.GameDetails.Date;
                                break;
                            case "Result":
                                tempLabel.Text = item.GameDetails.GetResultString();
                                break;
                        }
                        tempLabel.Size = column.TitleLabel.Size - new Size(0, 3)
                            + new Size(column.NextSplit.Width + 3, 0);
                        tempLabel.Location = column.TitleLabel.Location + new Size(0, 
                            (TempList.IndexOf(item) * (tempLabel.Size.Height - 4)) + 40);
                        //item.RecordLabelSet.Labels.Add(Tuple.Create(tempLabel, column));
                        DBEPanel.Controls.Add(tempLabel);
                    }
                }
            DBEPanel.ResumeLayout();
            sw.Stop();
            DBExplorer.Form.ShowDialog(this);
        }
        void Split_MouseUp(object sender, MouseEventArgs e)
        {
            SplitLabelClicked = null;
        }
        void Split_MouseMove(object sender, MouseEventArgs e)
        {
            if (SplitLabelClicked == null)
                return;
            Point diff = SplitLocation - new Size(e.Location);
            SplitLabelClicked.Location -= new Size(diff.X, 0);
            UpdateSplitResize(SplitLabelClicked, diff.X);
        }
        void Split_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            SplitLabelClicked = sender as Label;
            SplitLocation = e.Location;
        }
        private void LoadPgnGame()
        {
            String buffer = ClipBoardBuffer, str = "";
            int x;
            GameDetails gd = gameDetails;

            x = buffer.IndexOf('[');
            if (x < 0)
            {
                gameDetails = gd;
                gameDetails.isUserGame = true;
                MessageBox.Show("Game Load Failed!. \nWrong PGN text format!");
                return;
            }
            str = buffer.Substring(0, x);
            buffer = buffer.Substring(x);
            gameDetails = new GameDetails();
            gameDetails.isUserGame = false;
            buffer = TagScanner(buffer, gameDetails);
            if (buffer == "")
            {
                gameDetails = gd;
                gameDetails.isUserGame = true;
                return;
            }
            
            if (!MoveTextScanner(buffer))
            {
                gameDetails = gd;
                gameDetails.isUserGame = true;
                MessageBox.Show("Game Load Failed!. \nWrong PGN text format!");
                return;
            }
    }
        private bool MoveTextScanner(string buffer)
        {
            bool newVal = false, endVal = false, isShortComment = false,
                isLongComment = false, isTradComm = false;
            while (buffer != "")
            {
                int i = 0;
                Piece.PieceType pieceType = Piece.PieceType.Pawn;
                String initSq = "", destSq = "", promoteInitials = "";
                bool START = false;
                String str = "";
                foreach (var item in buffer)
                {
                    if (isLongComment && item != '}')
                    {
                        str += item;
                        i++;
                        continue;
                    }
                    else if (item == '}')
                    {
                        i++;
                        break;
                    }

                    if (isShortComment)
                    {
                        if (char.IsNumber(item))
                            str += item;
                        else
                            break;
                    }
                    else if (item == '$')
                    {
                        isShortComment = true;
                        START = false;
                    }

                    else if (START)
                    {
                        if (char.IsWhiteSpace(item) || item == ')')
                            break;
                        else
                            str += item;
                    }
                    else if (char.IsLetter(item))
                    {
                        START = true;
                        str += item;
                    }
                    else if (isTradComm)
                    {
                        if (item == '!' || item == '?')
                        {
                            str += item;
                            i++;
                            break;
                        }
                        else
                            break;
                    }
                    else if (item == '!' || item == '?')
                    {
                        isTradComm = true;
                        str += item;
                        i++;
                        continue;
                    }
                    else if (item == '{')
                    {
                        isLongComment = true;
                        isShortComment = false;
                        START = false;
                    }
                    else if (item == '(')
                    {
                        newVal = true;
                        i++;
                        break;
                    }
                    else if (item == ')')
                    {
                        endVal = true;
                        i++;
                        break;
                    }
                    i++;
                }
                buffer = buffer.Substring(i);

                if (newVal)
                {
                    newVal = false;
                    Position nextPosition = CurrentVariation.MovesList
                        [CurrentVariation.MovesList.IndexOf(CurrentPosition)];
                    LoadPosition
                        (CurrentVariation.MovesList
                        [CurrentVariation.MovesList.IndexOf(CurrentPosition) - 1], true);

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
                    continue;
                }
                else if (endVal)
                {
                    endVal = false;
                    if (CurrentVariation.ParentLine == null)
                        return false;
                    CurrentVariation = CurrentVariation.ParentLine;
                    LoadPosition(CurrentVariation.MovesList
                        [CurrentVariation.MovesList.Count - 1], true);
                    continue;
                }

                else if (isLongComment)
                {
                    isLongComment = false;
                    CurrentPosition.LastMovePlayed.Comment.LongComment = str;
                    continue;
                }

                else if (isShortComment)
                {
                    isShortComment = false;
                    int a;
                    if (int.TryParse(str, out a))
                    {
                        switch (a)
                        {
                            case 1: CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.StrongMove;
                                break;
                            case 2: CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.WeakMove;
                                break;
                            case 3: CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.GreatMove;
                                break;
                            case 4: CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.TerribleMove;
                                break;
                            case 5: CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.InterestingMove;
                                break;
                            case 6: CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.DubiousMove;
                                break;
                            case 11: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.EqualPosition;
                                break;
                            case 14: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.SlightWhiteAdv;
                                break;
                            case 15: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.SlightBlackAdv;
                                break;
                            case 16: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.ModerateWhiteAdv;
                                break;
                            case 17: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.ModerateBlackAdv;
                                break;
                            case 18: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.DecisiveWhiteAdv;
                                break;
                            case 19: CurrentPosition.LastMovePlayed.Comment.PositionComment
                                = PositionComment.DecisiveBlackAdv;
                                break;
                        }
                    }
                    continue;
                }
                else if (isTradComm)
                {
                    isTradComm = false;
                    if (CurrentPosition.LastMovePlayed != null)
                    {
                        if (str == "!!")
                            CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.GreatMove;
                        else if (str == "??")
                            CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.TerribleMove;
                        else if (str == "!?")
                            CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.InterestingMove;
                        else if (str == "?!")
                            CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.DubiousMove;
                        else if (str == "!")
                            CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.StrongMove;
                        else if (str == "?")
                            CurrentPosition.LastMovePlayed.Comment.MoveComment
                                = MoveComment.WeakMove;
                    }
                    continue;
                }

                if (str.Contains("O-O-O"))
                {
                    if (!PlayOutMove(Piece.PieceType.King, (sideToPlay == Piece.PieceSide.White ? "e1" : "e8"),
                        (sideToPlay == Piece.PieceSide.White ? "c1" : "c8"), "", str))
                        return false;
                    else
                        continue;
                }
                if (str.Contains("O-O"))
                {
                    if (!PlayOutMove(Piece.PieceType.King, (sideToPlay == Piece.PieceSide.White ? "e1" : "e8"),
                        (sideToPlay == Piece.PieceSide.White ? "g1" : "g8"), "", str))
                        return false;
                    else
                        continue;
                }

                if (str == "")
                    return true;
                if (char.IsUpper(str[0]))
                {
                    if (str[0] == 'N')
                        pieceType = Piece.PieceType.Knight;
                    else if (str[0] == 'B')
                        pieceType = Piece.PieceType.Bishop;
                    else if (str[0] == 'R')
                        pieceType = Piece.PieceType.Rook;
                    else if (str[0] == 'Q')
                        pieceType = Piece.PieceType.Queen;
                    else if (str[0] == 'K')
                        pieceType = Piece.PieceType.King;
                    else if (str[0] == 'P')
                        pieceType = Piece.PieceType.Pawn; // This should support Promotions

                    int x = str.IndexOf('x');
                    if (x > 0)
                    {
                        destSq = str[x + 1].ToString() + str[x + 2];
                        if (x == 2)
                            initSq = str[1].ToString();
                        else if (x == 3)
                            initSq = str[1].ToString() + str[2];
                    }
                    else
                    {
                        for (x = str.Length - 1; x > 0; x--)
                        {
                            if (char.IsLetter(str[x]))
                                break;
                        }
                        destSq = str[x].ToString() + str[x + 1];
                        if (x == 2)
                            initSq = str[1].ToString();
                        else if (x == 3)
                            initSq = str[1].ToString() + str[2];
                    }
                }
                else
                {
                    pieceType = Piece.PieceType.Pawn;
                    if (char.IsNumber(str[1]))
                    {
                        destSq = str[0].ToString() + str[1];
                        if (str[1] == '8' || str [1] == '1')
                        {
                            if (str[2] == '=')
                                promoteInitials = str[3].ToString();
                            else
                                promoteInitials = str[2].ToString();
                        }
                    }
                    else
                    {
                        initSq = str[0].ToString();
                        destSq = str[2].ToString() + str[3];
                        if (str[3] == '8' || str[3] == '1')
                        {
                            if (str[4] == '=')
                                promoteInitials = str[5].ToString();
                            else
                                promoteInitials = str[4].ToString();
                        }
                    }
                }
                if (!PlayOutMove(pieceType, initSq, destSq, promoteInitials, str))
                    return false;
                else
                    continue;
            }
            return true;
        }
        private bool PlayOutMove(Piece.PieceType pieceType, string initSq, string destSq, string promoteStr, string str)
        {
            Square square = null;
            if (!TryGetSquare(destSq, out square))
                return false;
            List<Piece> TempList = new List<Piece>();
            foreach (var item in Pieces)
            {
                if (item.Type == pieceType && item.Side == sideToPlay)
                    TempList.Add(item);
            }
            if (TempList.Count == 0)
                return false;
            else
            {
                foreach (var item in TempList)
                {
                    Move move = new Move(item, square);
                    if (pieceType == Piece.PieceType.Pawn && promoteStr != "")
                    {
                        switch (promoteStr)
                        {
                            case "N": move.PromoteType = Move.ItsPromoteType.Knight; break;
                            case "B": move.PromoteType = Move.ItsPromoteType.Bishop; break;
                            case "R": move.PromoteType = Move.ItsPromoteType.Rook; break;
                            case "Q": move.PromoteType = Move.ItsPromoteType.Queen; break;
                            default:
                                return false;
                        }
                    }
                    if (initSq != "")
                    {
                        if (item.Square.Name.Contains(initSq))
                        {
                            if (CheckMove(move))
                            {
                                MakeMove(move, panel1.CreateGraphics(), true);

                                if (str.Contains("!!"))
                                    CurrentPosition.LastMovePlayed.Comment.MoveComment
                                        = MoveComment.GreatMove;
                                else if (str.Contains("??"))
                                    CurrentPosition.LastMovePlayed.Comment.MoveComment
                                        = MoveComment.TerribleMove;
                                else if (str.Contains("!?"))
                                    CurrentPosition.LastMovePlayed.Comment.MoveComment
                                        = MoveComment.InterestingMove;
                                else if (str.Contains("?!"))
                                    CurrentPosition.LastMovePlayed.Comment.MoveComment
                                        = MoveComment.DubiousMove;
                                else if (str.Contains("!"))
                                    CurrentPosition.LastMovePlayed.Comment.MoveComment
                                        = MoveComment.StrongMove;
                                else if (str.Contains("?"))
                                    CurrentPosition.LastMovePlayed.Comment.MoveComment
                                        = MoveComment.WeakMove;
                                return true;
                            }
                        }
                    }
                    else    // initSq is blank
                    {
                        if (CheckMove(move))
                        {
                            MakeMove(move, panel1.CreateGraphics(), true);

                            if (str.Contains("!!"))
                                CurrentPosition.LastMovePlayed.Comment.MoveComment
                                    = MoveComment.GreatMove;
                            else if (str.Contains("??"))
                                CurrentPosition.LastMovePlayed.Comment.MoveComment
                                    = MoveComment.TerribleMove;
                            else if (str.Contains("!?"))
                                CurrentPosition.LastMovePlayed.Comment.MoveComment
                                    = MoveComment.InterestingMove;
                            else if (str.Contains("?!"))
                                CurrentPosition.LastMovePlayed.Comment.MoveComment
                                    = MoveComment.DubiousMove;
                            else if (str.Contains("!"))
                                CurrentPosition.LastMovePlayed.Comment.MoveComment
                                    = MoveComment.StrongMove;
                            else if (str.Contains("?"))
                                CurrentPosition.LastMovePlayed.Comment.MoveComment
                                    = MoveComment.WeakMove;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private String TagScanner(string buffer, GameDetails gd)
        {
            gd.WhitePlayer.Rating = 0;
            gd.BlackPlayer.Rating = 0;
            String str = "", TagName, TagValue;
            while (true)
            {
                str = "";
                int i = 0;
                buffer = buffer.Substring(1);
                foreach (var item in buffer)
                {
                    if (item == ' ')
                        break;
                    str += item;
                    i++;
                }
                TagName = str;
                buffer = buffer.Substring(i);
                if (buffer == "")
                {
                    return buffer;
                }
                str = "";
                i = 0;
                bool START = false;
                foreach (var item in buffer)
                {
                    if (START && item != '"' && item != '[' && item != ']')
                        str += item;
                    else if (START && (item == '"' || item == '[' || item == ']'))
                        break;
                    else if (item == '"')
                        START = true;
                    i++;
                }
                buffer = buffer.Substring(i);
                if (buffer == "")
                {
                    return buffer;
                }
                TagValue = str;
                if (String.Compare(TagName, "Event", true) == 0)
                    gd.Event = TagValue;
                else if (String.Compare(TagName, "Site", true) == 0)
                    gd.Site = TagValue;
                else if (String.Compare(TagName, "Date", true) == 0)
                {
                    String s = "";
                    gd.Date = TryConvertDate(TagValue, out s) ? s : "";

                }
                else if (String.Compare(TagName, "FEN", true) == 0)
                {

                }
                else if (String.Compare(TagName, "ECO", true) == 0)
                    gd.ECO = TagValue;
                else if (String.Compare(TagName, "White", true) == 0)
                    gd.WhitePlayer.Name = TagValue;
                else if (String.Compare(TagName, "Black", true) == 0)
                    gd.BlackPlayer.Name = TagValue;
                else if (String.Compare(TagName, "WhiteElo", true) == 0)
                {
                    int x;
                    if (int.TryParse(TagValue, out x))
                        gd.WhitePlayer.Rating = x;
                }
                else if (String.Compare(TagName, "BlackElo", true) == 0)
                {
                    int x;
                    if (int.TryParse(TagValue, out x))
                        gd.BlackPlayer.Rating = x;
                }
                else if (String.Compare(TagName, "Round", true) == 0)
                {
                    float x;
                    if (float.TryParse(TagValue, out x))
                        gd.Round = (int)x;
                }
                else if (String.Compare(TagName, "Result", true) == 0)
                {
                    if (String.Compare(TagValue, "1-0", true) == 0)
                        gd.Result = GameDetails.Outcome.WhiteWin;
                    else if (String.Compare(TagValue, "0-1", true) == 0)
                        gd.Result = GameDetails.Outcome.BlackWin;
                    else if (String.Compare(TagValue, "1/2-1/2", true) == 0)
                        gd.Result = GameDetails.Outcome.Draw;
                    else if (String.Compare(TagValue, "*", true) == 0)
                        gd.Result = GameDetails.Outcome.NotAvailable;
                }
                i = 0;
                START = false;
                foreach (var item in buffer)
                {
                    if (!String.IsNullOrWhiteSpace(item.ToString()) && item != '"'
                        && item != ']' && item != '[')
                        return buffer;
                    if (item == '[')
                    {
                        START = true;
                        break;
                    }
                    i++;
                }
                if (START)
                    buffer = buffer.Substring(i);
                else if (buffer == "")
                {
                    return buffer;
                }
            }
        }
        private bool LoadDatabase(string buffer)
        {
            if (buffer.IndexOf("\n[") <= 0)
            {
                MessageBox.Show("Game Load Failed!. \nWrong PGN text format!");
                return false;
            }
            int j = 0;
            List<String> StrList = new List<string>();
            DBExplorer.Games = new List<Game>();
            if (buffer.Length > 50000)
            {
                String tempStr;
                for (int i = 0; i < (buffer.Length / 50000) + 1; i++)
                {
                    if (buffer.Length >= (i + 1) * 50000)
                        tempStr = buffer.Substring(i * 50000, 50000);
                    else
                        tempStr = buffer.Substring(i * 50000);
                    StrList.Add(tempStr);
                }
            }
            else
                StrList.Add(buffer);

            String spareStr = "";
            int x2 = 0;
            for (int i = 0; i < StrList.Count; i++)
            {
                String item = spareStr + StrList[i];
                int x1 = item.IndexOf("\n[") + 1;
                if (spareStr != "")
                {
                    if (x2 == -100)
                    {
                        DBExplorer.Games[DBExplorer.Games.Count - 1].MoveText = 
                            item.Substring(0, x1);
                    }
                    //populate moveText for previous Game object
                    //Only if spareStr isn't "[Event . . . "
                }
                item = item.Substring(x1 + 1);
                x1 = 0;

                while (true)
                {
                    x2 = item.IndexOf("\n1");
                    if (x2 < 0)
                    {
                        spareStr = item;
                        break;
                    }
                    Game game = new Game();
                    game.GameDetails = new GameDetails();
                    String str = item.Substring(0, x2);
                    TagScanner(str, game.GameDetails);
                    DBExplorer.Games.Add(game);
                    j++;
                    item = item.Substring(x2 + 1);
                    x1 = item.IndexOf("\n[");
                    if (x1 < 0)
                    {

                        if (i != StrList.Count - 1)
                        {
                            spareStr = item;
                            x2 = -100;
                            break; 
                        }
                        else
                        {
                            game.MoveText = item;
                            break;
                        }
                    }
                    game.MoveText = item.Substring(0, x1);
                    item = item.Substring(x1 + 1);
                    x1 = 0;
                }
            }
            return true;
        }
        private void CreateBook(List<Game> list)
        {
            EnginePanel1.Refresh();
            List<Game> tempList = new List<Game>();
            int i = 0;
            foreach (var item in list)
            {
                if (!MoveTextScanner(item.MoveText))
                    tempList.Add(item);
                else
                    item.MainLine = MainLine;
                i++;
                
            }
            foreach (var item in tempList)
                list.Remove(item);
        }
        public interface IDataBase
        {
            SortLogic SortLogic { get; set; }
            SortOrder SortOrder { get; set; }
        }
        [Serializable]
        public class Game
        {
            public Game()
            {
                GameDetails = new GameDetails();
            }
            public List<Piece> Pieces { get; set; }
            public List<Piece> CapturedPieces { get; set; }
            public GameDetails GameDetails { get; set; }
            public VariationHolder MainLine { get; set; }
            public List<VariationHolder> GameVariations { get; set; }
            public Position CurrentPosition { get; set; }
            public VariationHolder CurrentVariation { get; set; }
            public String MoveText { get; set; }
        }
        public class DatabaseExplorer : IComparer<Game>, IDataBase
        {
            public DatabaseExplorer()
            {
                Games = new List<Game>();
                Form = new BufferedForm();
            }
            public List<Game> Games { get; set; }
            public SortLogic SortLogic { get; set; }
            public SortOrder SortOrder { get; set; }
            public BufferedForm Form { get; set; }
            public int Compare(Game x, Game y)
            {
                switch (SortLogic)
                {
                    case SortLogic.White:
                        return String.Compare(x.GameDetails.WhitePlayer.Name, 
                            y.GameDetails.WhitePlayer.Name, true);
                    case SortLogic.Black:
                        return String.Compare(x.GameDetails.BlackPlayer.Name, 
                            y.GameDetails.BlackPlayer.Name, true);
                    case SortLogic.Result:
                        if (x.GameDetails.Result == y.GameDetails.Result)
                            return 0;
                        if (x.GameDetails.Result < y.GameDetails.Result)
                            return -1;
                            return 1;
                    case SortLogic.Date:
                        return String.Compare(x.GameDetails.Date,
                            y.GameDetails.Date, true);
                    case SortLogic.Event:
                        return String.Compare(x.GameDetails.Event,
                            y.GameDetails.Event, true);
                    case SortLogic.Round:
                        return String.Compare(x.GameDetails.Round.ToString(),
                            y.GameDetails.Round.ToString(), true);
                    case Form1.SortLogic.ECO:
                        return String.Compare(x.GameDetails.ECO,
                            y.GameDetails.ECO, true);
                    default: return 0;
                }
            }
        }
        [Serializable]
        public class RecordLabelSet
        {
            public RecordLabelSet(Game game)
            {
                Labels = new List<Tuple<Label, Column>>();
                Game = game;
            }
            public List<Tuple<Label, Column>> Labels { get; set; }
            public Game Game { get; set; }
            public bool IsSelected { get; set; }
            public bool IsHighlighted { get; set; }
        }
        public class Column
        {
            public Column PreviousColumn { get; set; }
            public Column NextColumn { get; set; }
            public int Width { get; set; }
            public String Name { get; set; }
            public Label TitleLabel { get; set; }
            public Label NextSplit { get; set; }
        }
        public class BufferedForm : Form
        {
            public BufferedForm() : base()
            {
                DoubleBuffered = true;
            }
        }
        [Serializable]
        public class OpeningNode
        {
            public OpeningNode() 
            {
                Name = "";
                ECOCode = "";
            }
            public void CreateStringTuple()
            {
                if (pieceInfos == null)
                    return;
                StringTuple = new List<Tuple<string,string>>();
                foreach (var item in pieceInfos)
                {
                    StringTuple.Add(Tuple.Create(item.Piece.Name, item.Square.Name));
                }
            }
            public override string ToString()
            {
                return ECOCode + " \t" + Name +" \t" + OpeningLine;
            }
            public OpeningNode Parent { get; set; }
            public List<OpeningNode> Children { get; set; }
            public Piece.PieceSide SideToPlay { get; set; }
            public double Probability { get; set; }
            public String OpeningLine { get; set; }
            public List<Tuple<String, String>> StringTuple { get; set; }
            [NonSerialized]
            List<PieceInfo> pieceInfos;
            public List<PieceInfo> PieceInfos { get { return pieceInfos; } set { pieceInfos = value; } }
            public int WinCount { get; set; }
            public int LossCount { get; set; }
            public int DrawCount { get; set; }
            public int PlyCount { get; set; }
            public String ECOCode { get; set; }
            public String Name { get; set; }
        }
    }
}
