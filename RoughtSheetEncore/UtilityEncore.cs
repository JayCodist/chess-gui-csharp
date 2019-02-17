using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace RoughtSheetEncore
{
    public partial class Form1 : Form
    {
        [Serializable]
        public enum ChessFont
        {
            Merida,
            Berlin,
            Alpha,
            Kingdom,
            Leipzig,
            Marroquin,
            Maya,
            Usual
        }

        Form TimeForm, SettingsForm, HashOptionsForm;
        TimeControl timeControl;
        bool isTimePaused = true, PausePending, isRatedGameInProgress, HashSettingFormDelay;
        State state;
        Settings settings;
        ChessFont PieceSet = ChessFont.Merida;


        private void ShowSettingsForm()
        {
            SettingsForm = new Form();
            SettingsForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            SettingsForm.StartPosition = FormStartPosition.CenterParent;
            SettingsForm.ShowIcon = false;
            SettingsForm.Text = "Preferences";
            SettingsForm.Size = new System.Drawing.Size(700, 600);
            SettingsForm.MaximizeBox = false;
            SettingsForm.MinimizeBox = false;
            SettingsForm.KeyPreview = true;
            SettingsForm.KeyDown += SettingsForm_KeyDown;
            //SettingsForm.BackColor = Color.Silver;
            SettingsForm.BackColor = Color.LightGray;
            var S_ControlList = new List<Control>();

            Font font = new System.Drawing.Font("Segoe UI", 15, FontStyle.Bold);
            Font font2 = new System.Drawing.Font("Segoe UI", 12, FontStyle.Regular);
            Color color = Color.DarkRed;

            #region Tabs
            Label AppearLabel = new Label();
            AppearLabel.Text = "Appearance";
            AppearLabel.Location = new Point(0, 50);
            AppearLabel.Cursor = Cursors.Hand;
            AppearLabel.MouseEnter += SettingLabel_MouseEnter;
            AppearLabel.MouseLeave += SettingLabel_MouseLeave;
            AppearLabel.Click += SettingLabel_Click;
            AppearLabel.TextAlign = ContentAlignment.MiddleRight;
            AppearLabel.BackColor = Color.DarkRed;
            AppearLabel.AutoSize = false;
            AppearLabel.Size = new System.Drawing.Size(200, 50);
            AppearLabel.ForeColor = Color.OldLace;
            AppearLabel.Font = font;

            Panel panel = new Panel();
            panel.Size = new System.Drawing.Size(AppearLabel.Width, SettingsForm.Height);
            panel.Location = new Point(0, 0);
            panel.BackColor = AppearLabel.BackColor;

            Label BehaveLabel = new Label();
            BehaveLabel.Text = "Behaviour";
            BehaveLabel.Location = new Point(AppearLabel.Left, AppearLabel.Bottom);
            BehaveLabel.Cursor = Cursors.Hand;
            BehaveLabel.MouseEnter += SettingLabel_MouseEnter;
            BehaveLabel.MouseLeave += SettingLabel_MouseLeave;
            BehaveLabel.Click += SettingLabel_Click;
            BehaveLabel.TextAlign = ContentAlignment.MiddleRight;
            BehaveLabel.BackColor = Color.DarkRed;
            BehaveLabel.AutoSize = false;
            BehaveLabel.Size = new System.Drawing.Size(200, 50);
            BehaveLabel.ForeColor = Color.OldLace;
            BehaveLabel.Font = font;

            Label EngineLabel = new Label();
            EngineLabel.Text = "Engines";
            EngineLabel.Location = new Point(AppearLabel.Left, BehaveLabel.Bottom);
            EngineLabel.Cursor = Cursors.Hand;
            EngineLabel.MouseEnter += SettingLabel_MouseEnter;
            EngineLabel.MouseLeave += SettingLabel_MouseLeave;
            EngineLabel.Click += SettingLabel_Click;
            EngineLabel.TextAlign = ContentAlignment.MiddleRight;
            EngineLabel.BackColor = Color.DarkRed;
            EngineLabel.AutoSize = false;
            EngineLabel.Size = new System.Drawing.Size(200, 50);
            EngineLabel.ForeColor = Color.OldLace;
            EngineLabel.Font = font;

            Label MiscLabel = new Label();
            MiscLabel.Text = "Miscellaneous";
            MiscLabel.Location = new Point(AppearLabel.Left, EngineLabel.Bottom);
            MiscLabel.Cursor = Cursors.Hand;
            MiscLabel.MouseEnter += SettingLabel_MouseEnter;
            MiscLabel.MouseLeave += SettingLabel_MouseLeave;
            MiscLabel.Click += SettingLabel_Click;
            MiscLabel.TextAlign = ContentAlignment.MiddleRight;
            MiscLabel.BackColor = Color.DarkRed;
            MiscLabel.AutoSize = false;
            MiscLabel.Size = new System.Drawing.Size(200, 50);
            MiscLabel.ForeColor = Color.OldLace;
            MiscLabel.Font = font;
#endregion

            #region AppearPanel
            Panel AppearPanel = new Panel();
            AppearPanel.Location = new Point(panel.Right, 0);
            AppearPanel.AutoScroll = true;
            AppearPanel.Size = new System.Drawing.Size(483, SettingsForm.Height - 100);
            AppearLabel.Tag = AppearPanel;
            AppearPanel.BackColor = Color.LightGray;
            AppearPanel.Paint += SettingPanel_Paint;
            List<Panel> RectListA = new List<Panel>();

            #region Themes
            Label PieceSetLabel = new Label();
            ComboBox PieceSetCB = new ComboBox();
            PieceSetLabel.Text = "Piece Set";
            PieceSetCB.ForeColor = color;
            PieceSetCB.Name = "PieceSetCB";
            PieceSetCB.FlatStyle = FlatStyle.Flat;
            PieceSetCB.DropDownStyle = ComboBoxStyle.DropDownList;
            PieceSetCB.Items.Add(ChessFont.Alpha);
            PieceSetCB.Items.Add(ChessFont.Berlin);
            PieceSetCB.Items.Add(ChessFont.Kingdom);
            PieceSetCB.Items.Add(ChessFont.Leipzig);
            PieceSetCB.Items.Add(ChessFont.Marroquin);
            PieceSetCB.Items.Add(ChessFont.Maya);
            PieceSetCB.Items.Add(ChessFont.Merida);
            PieceSetCB.Items.Add(ChessFont.Usual);
            PieceSetCB.SelectedItem = PieceSet;
            S_ControlList.Add(PieceSetCB);
            PieceSetCB.Cursor = Cursors.Hand;
            PieceSetLabel.ForeColor = color;
            PieceSetLabel.Font = font2;
            PieceSetCB.Font = font2;
            PieceSetLabel.AutoSize = true;
            PieceSetLabel.Location = new Point(80, 30);
            PieceSetCB.Location = new Point(PieceSetLabel.Right + 15, PieceSetLabel.Top - 3);
            AppearPanel.Controls.Add(PieceSetCB);
            AppearPanel.Controls.Add(PieceSetLabel);

            ComboBox ColorThemeCB = new ComboBox();
            Label ColorThemeLabel = new Label();
            ColorThemeCB.Name = "ColorThemeCB";
            S_ControlList.Add(ColorThemeCB);
            ColorThemeLabel.Text = "Colour Theme";
            ColorThemeCB.ForeColor = color;
            ColorThemeCB.FlatStyle = FlatStyle.Flat;
            ColorThemeLabel.ForeColor = color;
            ColorThemeLabel.Font = font2;
            ColorThemeLabel.AutoSize = true;
            ColorThemeLabel.Location = new Point(PieceSetLabel.Left, PieceSetLabel.Bottom + 20);
            ColorThemeCB.Location = new Point(ColorThemeLabel.Right + 15, ColorThemeLabel.Top - 3);
            ColorThemeCB.Items.Add(PresetTheme.Theme.Brick);
            ColorThemeCB.Items.Add(PresetTheme.Theme.Coffee);
            ColorThemeCB.Items.Add(PresetTheme.Theme.Linen);
            ColorThemeCB.Items.Add(PresetTheme.Theme.Metro);
            ColorThemeCB.Items.Add(PresetTheme.Theme.Silver);
            ColorThemeCB.Items.Add(PresetTheme.Theme.Wood);
            ColorThemeCB.SelectedItem = ColorTheme.ColorTheme;
            ColorThemeCB.Font = font2;
            ColorThemeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            ColorThemeCB.Cursor = Cursors.Hand;
            AppearPanel.Controls.Add(ColorThemeCB);
            AppearPanel.Controls.Add(ColorThemeLabel);

            ComboBox AnimationCB = new ComboBox();
            Label AnimationLabel = new Label();
            S_ControlList.Add(AnimationCB);
            AnimationCB.Name = "AnimationCB";
            AnimationLabel.Text = "Animation";
            AnimationCB.ForeColor = color;
            AnimationCB.FlatStyle = FlatStyle.Flat;
            AnimationLabel.ForeColor = color;
            AnimationLabel.Font = font2;
            AnimationLabel.AutoSize = true;
            AnimationLabel.Location = new Point(PieceSetLabel.Left, ColorThemeLabel.Bottom + 20);
            AnimationCB.Location = new Point(AnimationLabel.Right + 15, AnimationLabel.Top - 3);
            AnimationCB.Items.Add("None");
            AnimationCB.Items.Add("Fast");
            AnimationCB.Items.Add("Slow");
            AnimationCB.SelectedItem = settings.appearance.AnimationType;
            AnimationCB.Font = font2;
            AnimationCB.DropDownStyle = ComboBoxStyle.DropDownList;
            AnimationCB.Cursor = Cursors.Hand;
            AppearPanel.Controls.Add(AnimationCB);
            AppearPanel.Controls.Add(AnimationLabel);

            CheckBox ShouldShowLegalMovesCB = new CheckBox();
            S_ControlList.Add(ShouldShowLegalMovesCB);
            ShouldShowLegalMovesCB.Text = "Show Legal Moves";
            ShouldShowLegalMovesCB.Font = font2;
            ShouldShowLegalMovesCB.ForeColor = color;
            ShouldShowLegalMovesCB.Cursor = Cursors.Hand;
            ShouldShowLegalMovesCB.Checked = settings.appearance.ShouldShowLegalMovesHL;
            ShouldShowLegalMovesCB.Location = new Point(105, AnimationCB.Bottom + 25);
            ShouldShowLegalMovesCB.AutoSize = true;
            AppearPanel.Controls.Add(ShouldShowLegalMovesCB);
            #endregion

            #region LastMove
            Panel LastMovePanel = new Panel();
            LastMovePanel.Location = new Point(35, ShouldShowLegalMovesCB.Bottom + 25);
            LastMovePanel.Size = new System.Drawing.Size(AppearPanel.Width - 70, 100);
            RectListA.Add(LastMovePanel);
            AppearPanel.Tag = RectListA;
            AppearPanel.Controls.Add(LastMovePanel);

            CheckBox ShouldShowLastMoveCB = new CheckBox();
            S_ControlList.Add(ShouldShowLastMoveCB);
            ShouldShowLastMoveCB.Font = font2;
            ShouldShowLastMoveCB.Text = "Highlight last move played";
            ShouldShowLastMoveCB.AutoSize = true;
            ShouldShowLastMoveCB.Checked = settings.appearance.ShouldShowLastMoveHL;
            ShouldShowLastMoveCB.Location = new Point(60, 10);
            ShouldShowLastMoveCB.CheckedChanged += SettingsCheckBox_CheckedChanged;
            ShouldShowLastMoveCB.Cursor = Cursors.Hand;
            ShouldShowLastMoveCB.ForeColor = color;
            LastMovePanel.Controls.Add(ShouldShowLastMoveCB);

            ComboBox LastMoveTypeCB = new ComboBox();
            S_ControlList.Add(LastMoveTypeCB);
            LastMoveTypeCB.Name = "LastMoveTypeCB";
            Label LastMoveTypeLabel = new Label();
            LastMoveTypeLabel.Text = "Highlight type";
            LastMoveTypeCB.ForeColor = color;
            LastMoveTypeCB.FlatStyle = FlatStyle.Flat;
            LastMoveTypeLabel.ForeColor = color;
            LastMoveTypeLabel.Font = font2;
            LastMoveTypeLabel.Enabled = ShouldShowLastMoveCB.Checked;
            LastMoveTypeCB.Enabled = ShouldShowLastMoveCB.Checked;
            LastMoveTypeLabel.AutoSize = true;
            LastMoveTypeLabel.Location = new Point(PieceSetLabel.Left, ShouldShowLastMoveCB.Bottom + 20);
            LastMoveTypeCB.Location = new Point(LastMoveTypeLabel.Right + 15, LastMoveTypeLabel.Top - 3);
            LastMoveTypeCB.Items.Add("Shade");
            LastMoveTypeCB.Items.Add("Border");
            LastMoveTypeCB.SelectedItem = settings.appearance.LastMoveHLType;
            LastMoveTypeCB.Font = font2;
            LastMoveTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            LastMoveTypeCB.Cursor = Cursors.Hand;
            List<Control> list = new List<Control>();
            list.Add(LastMoveTypeCB);
            list.Add(LastMoveTypeLabel);
            ShouldShowLastMoveCB.Tag = list;
            LastMovePanel.Controls.Add(LastMoveTypeCB);
            LastMovePanel.Controls.Add(LastMoveTypeLabel);

            #endregion

            #region KICHL
            Panel KICHLPanel = new Panel();
            KICHLPanel.Location = new Point(35, LastMovePanel.Bottom + 30);
            KICHLPanel.Size = new System.Drawing.Size(AppearPanel.Width - 70, 100);
            RectListA.Add(KICHLPanel);
            AppearPanel.Tag = RectListA;
            AppearPanel.Controls.Add(KICHLPanel);

            CheckBox ShouldShowKICHLCB = new CheckBox();
            S_ControlList.Add(ShouldShowKICHLCB);
            ShouldShowKICHLCB.Font = font2;
            ShouldShowKICHLCB.Text = "Highlight King-in-check";
            ShouldShowKICHLCB.AutoSize = true;
            ShouldShowKICHLCB.Checked = settings.appearance.ShouldShowKICHL;
            ShouldShowKICHLCB.Cursor = Cursors.Hand;
            ShouldShowKICHLCB.CheckedChanged += SettingsCheckBox_CheckedChanged;
            ShouldShowKICHLCB.Location = new Point(60, 10);
            ShouldShowKICHLCB.ForeColor = color;
            KICHLPanel.Controls.Add(ShouldShowKICHLCB);

            ComboBox KICHLTypeCB = new ComboBox();
            S_ControlList.Add(KICHLTypeCB);
            KICHLTypeCB.Name = "KICHLTypeCB";
            Label KICHLTypeLabel = new Label();
            KICHLTypeLabel.Text = "Highlight type";
            KICHLTypeCB.ForeColor = color;
            KICHLTypeCB.FlatStyle = FlatStyle.Flat;
            KICHLTypeLabel.ForeColor = color;
            KICHLTypeLabel.Font = font2;
            KICHLTypeLabel.AutoSize = true;
            KICHLTypeLabel.Location = new Point(PieceSetLabel.Left, ShouldShowKICHLCB.Bottom + 20);
            KICHLTypeCB.Location = new Point(KICHLTypeLabel.Right + 15, KICHLTypeLabel.Top - 3);
            KICHLTypeCB.Items.Add("Glow");
            KICHLTypeCB.Items.Add("Border");
            KICHLTypeCB.SelectedItem = settings.appearance.KICHLType;
            KICHLTypeLabel.Enabled = ShouldShowKICHLCB.Checked;
            KICHLTypeCB.Enabled = ShouldShowKICHLCB.Checked;
            KICHLTypeCB.Font = font2;
            KICHLTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            KICHLTypeCB.Cursor = Cursors.Hand;
            List<Control> list2 = new List<Control>();
            list2.Add(KICHLTypeCB);
            list2.Add(KICHLTypeLabel);
            ShouldShowKICHLCB.Tag = list2;
            KICHLPanel.Controls.Add(KICHLTypeCB);
            KICHLPanel.Controls.Add(KICHLTypeLabel);
            #endregion

            #region UserArrow
            Panel UserArrowPanel = new Panel();
            UserArrowPanel.Location = new Point(35, KICHLPanel.Bottom + 30);
            UserArrowPanel.Size = new System.Drawing.Size(AppearPanel.Width - 70, 150);
            RectListA.Add(UserArrowPanel);
            AppearPanel.Tag = RectListA;
            AppearPanel.Controls.Add(UserArrowPanel);

            CheckBox ShouldShowUserArrowCB = new CheckBox();
            S_ControlList.Add(ShouldShowUserArrowCB);
            ShouldShowUserArrowCB.Font = font2;
            ShouldShowUserArrowCB.Text = "Show arrows drawn by user";
            ShouldShowUserArrowCB.AutoSize = true;
            ShouldShowUserArrowCB.Checked = settings.appearance.ShouldShowUserArrows;
            ShouldShowUserArrowCB.CheckedChanged += SettingsCheckBox_CheckedChanged;
            ShouldShowUserArrowCB.Cursor = Cursors.Hand;
            ShouldShowUserArrowCB.Location = new Point(60, 10);
            ShouldShowUserArrowCB.ForeColor = color;
            UserArrowPanel.Controls.Add(ShouldShowUserArrowCB);

            ComboBox UserArrowTypeCB = new ComboBox();
            S_ControlList.Add(UserArrowTypeCB);
            UserArrowTypeCB.Name = "UserArrowTypeCB";
            Label UserArrowTypeLabel = new Label();
            UserArrowTypeLabel.Text = "Square highlight type";
            UserArrowTypeCB.ForeColor = color;
            UserArrowTypeCB.FlatStyle = FlatStyle.Flat;
            UserArrowTypeLabel.ForeColor = color;
            UserArrowTypeLabel.Font = font2;
            UserArrowTypeLabel.AutoSize = true;
            UserArrowTypeLabel.Location = new Point(PieceSetLabel.Left - 30, ShouldShowUserArrowCB.Bottom + 20);
            UserArrowTypeCB.Location = new Point(UserArrowTypeLabel.Right + 65, UserArrowTypeLabel.Top - 3);
            UserArrowTypeCB.Items.Add("Shade");
            UserArrowTypeCB.Items.Add("Circle");
            UserArrowTypeCB.SelectedItem = settings.appearance.UserArrowSquareHLType;
            UserArrowTypeCB.Font = font2;
            UserArrowTypeCB.Enabled = ShouldShowUserArrowCB.Checked;
            UserArrowTypeLabel.Enabled = ShouldShowUserArrowCB.Checked;
            UserArrowTypeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            UserArrowTypeCB.Cursor = Cursors.Hand;
            UserArrowPanel.Controls.Add(UserArrowTypeCB);
            UserArrowPanel.Controls.Add(UserArrowTypeLabel);

            ComboBox UserArrowSaveCB = new ComboBox();
            S_ControlList.Add(UserArrowSaveCB);
            UserArrowSaveCB.Name = "UserArrowSaveCB";
            Label UserArrowSaveLabel = new Label();
            UserArrowSaveLabel.Text = "Save option";
            UserArrowSaveCB.ForeColor = color;
            UserArrowSaveCB.FlatStyle = FlatStyle.Flat;
            UserArrowSaveLabel.ForeColor = color;
            UserArrowSaveLabel.Font = font2;
            UserArrowSaveLabel.AutoSize = true;
            UserArrowSaveLabel.Location = new Point(PieceSetLabel.Left - 30, UserArrowTypeCB.Bottom + 30);
            UserArrowSaveCB.Location = new Point(UserArrowSaveLabel.Right + 17, UserArrowSaveLabel.Top - 3);
            UserArrowSaveCB.Items.Add("Save with position");
            UserArrowSaveCB.Items.Add("Don't save");
            UserArrowSaveCB.Enabled = ShouldShowUserArrowCB.Checked;
            UserArrowSaveLabel.Enabled = ShouldShowUserArrowCB.Checked;
            UserArrowSaveCB.SelectedItem = settings.appearance.SaveUserArrows;
            UserArrowSaveCB.Font = font2;
            UserArrowSaveCB.Width = 170;
            UserArrowSaveCB.DropDownStyle = ComboBoxStyle.DropDownList;
            List<Control> list3 = new List<Control>();
            list3.Add(UserArrowSaveCB);
            list3.Add(UserArrowSaveLabel);
            list3.Add(UserArrowTypeCB);
            list3.Add(UserArrowTypeLabel);
            ShouldShowUserArrowCB.Tag = list3;
            UserArrowSaveCB.Cursor = Cursors.Hand;
            UserArrowPanel.Controls.Add(UserArrowSaveCB);
            UserArrowPanel.Controls.Add(UserArrowSaveLabel);
            #endregion

            #region Last
            ComboBox BoardCordinatesCB = new ComboBox();
            S_ControlList.Add(BoardCordinatesCB);
            BoardCordinatesCB.Name = "BoardCordinatesCB";
            Label BoardCordinatesLabel = new Label();
            BoardCordinatesLabel.Text = "Board cordinates";
            BoardCordinatesCB.ForeColor = color;
            BoardCordinatesCB.FlatStyle = FlatStyle.Flat;
            BoardCordinatesLabel.ForeColor = color;
            BoardCordinatesLabel.Font = font2;
            BoardCordinatesLabel.AutoSize = true;
            BoardCordinatesLabel.Location = new Point(80, UserArrowPanel.Bottom + 20);
            BoardCordinatesCB.Location = new Point(BoardCordinatesLabel.Right + 65, BoardCordinatesLabel.Top - 3);
            BoardCordinatesCB.Items.Add("None");
            BoardCordinatesCB.Items.Add("Left and top");
            BoardCordinatesCB.Items.Add("All sides");
            BoardCordinatesCB.SelectedItem = settings.appearance.BoardCordinatesSides;
            BoardCordinatesCB.Font = font2;
            BoardCordinatesCB.DropDownStyle = ComboBoxStyle.DropDownList;
            BoardCordinatesCB.Cursor = Cursors.Hand;
            AppearPanel.Controls.Add(BoardCordinatesCB);
            AppearPanel.Controls.Add(BoardCordinatesLabel);

            CheckBox ShouldShowBMACB = new CheckBox();
            S_ControlList.Add(ShouldShowBMACB);
            ShouldShowBMACB.Text = "Show best-move arrow (during analysis)";
            ShouldShowBMACB.Checked = settings.appearance.ShouldShowBMA;
            ShouldShowBMACB.ForeColor = color;
            ShouldShowBMACB.Font = font2;
            ShouldShowBMACB.AutoSize = true;
            ShouldShowBMACB.Cursor = Cursors.Hand;
            ShouldShowBMACB.Location = new Point(80, BoardCordinatesCB.Bottom + 20);
            AppearPanel.Controls.Add(ShouldShowBMACB);
            #endregion

            Label tempLabel = new Label();
            tempLabel.Location = new Point(0, ShouldShowBMACB.Bottom + 30);
            AppearPanel.Controls.Add(tempLabel);
            #endregion

            #region BehavePanel

            #region First
            Panel BehavePanel = new Panel();
            BehavePanel.Location = new Point(panel.Right, 0);
            BehavePanel.AutoScroll = true;
            BehavePanel.Size = new System.Drawing.Size(487, SettingsForm.Height - 100);
            BehaveLabel.Tag = BehavePanel;
            BehavePanel.BackColor = Color.LightGray;
            BehavePanel.Paint += SettingPanel_Paint;
            List<Panel> RectListB = new List<Panel>();

            ComboBox PromotionCB = new ComboBox();
            S_ControlList.Add(PromotionCB);
            PromotionCB.Name = "PromotionCB";
            Label PromotionLabel = new Label();
            PromotionLabel.Text = "Pawn promotion";
            PromotionCB.ForeColor = color;
            PromotionCB.FlatStyle = FlatStyle.Flat;
            PromotionLabel.ForeColor = color;
            PromotionLabel.Font = font2;
            PromotionCB.Width = 130;
            PromotionLabel.AutoSize = true;
            PromotionLabel.Location = new Point(50, 30);
            PromotionCB.Location = new Point(PromotionLabel.Right + 70, PromotionLabel.Top - 3);
            PromotionCB.Items.Add("Always ask");
            PromotionCB.Items.Add("Always Queen");
            PromotionCB.SelectedItem = settings.behaviour.PromotionType;
            PromotionCB.Font = font2;
            PromotionCB.DropDownStyle = ComboBoxStyle.DropDownList;
            PromotionCB.Cursor = Cursors.Hand;
            BehavePanel.Controls.Add(PromotionCB);
            BehavePanel.Controls.Add(PromotionLabel);

            ComboBox NewMoveCB = new ComboBox();
            S_ControlList.Add(NewMoveCB);
            NewMoveCB.Name = "NewMoveCB";
            Label NewMoveLabel = new Label();
            NewMoveLabel.Text = "New Move Handling";
            NewMoveCB.ForeColor = color;
            NewMoveCB.FlatStyle = FlatStyle.Flat;
            NewMoveLabel.ForeColor = color;
            NewMoveLabel.Font = font2;
            NewMoveLabel.AutoSize = true;
            NewMoveLabel.Location = new Point(PromotionLabel.Left, PromotionLabel.Bottom + 20);
            NewMoveCB.Location = new Point(NewMoveLabel.Right + 70, NewMoveLabel.Top - 3);
            NewMoveCB.Width = 200;
            NewMoveCB.Items.Add("Always ask");
            NewMoveCB.Items.Add("Always new variation");
            NewMoveCB.Items.Add("Always overwrite");
            NewMoveCB.Items.Add("Always new main line");
            NewMoveCB.SelectedItem = settings.behaviour.NewMoveHandling;
            NewMoveCB.Font = font2;
            NewMoveCB.DropDownStyle = ComboBoxStyle.DropDownList;
            NewMoveCB.Cursor = Cursors.Hand;
            BehavePanel.Controls.Add(NewMoveCB);
            BehavePanel.Controls.Add(NewMoveLabel);
#endregion

            #region Etiquette
            Panel EtiquettePanel = new Panel();
            EtiquettePanel.Location = new Point(35, NewMoveCB.Bottom + 35);
            EtiquettePanel.Size = new System.Drawing.Size(AppearPanel.Width - 70, 100);
            RectListB.Add(EtiquettePanel);
            BehavePanel.Tag = RectListB;
            BehavePanel.Controls.Add(EtiquettePanel);

            CheckBox ShouldUseEtiquetteCB = new CheckBox();
            S_ControlList.Add(ShouldUseEtiquetteCB);
            ShouldUseEtiquetteCB.Font = font2;
            ShouldUseEtiquetteCB.Text = "Resign and offer draw";
            ShouldUseEtiquetteCB.AutoSize = true;
            ShouldUseEtiquetteCB.Checked = settings.behaviour.ResignAndOfferDraw;
            ShouldUseEtiquetteCB.Location = new Point(60, 10);
            ShouldUseEtiquetteCB.ForeColor = color;
            ShouldUseEtiquetteCB.Cursor = Cursors.Hand;
            EtiquettePanel.Controls.Add(ShouldUseEtiquetteCB);

            CheckBox ShouldClaimTimeForfeitCB = new CheckBox();
            S_ControlList.Add(ShouldClaimTimeForfeitCB);
            ShouldClaimTimeForfeitCB.Font = font2;
            ShouldClaimTimeForfeitCB.Text = "Claim forfeit on time";
            ShouldClaimTimeForfeitCB.AutoSize = true;
            ShouldClaimTimeForfeitCB.Checked = settings.behaviour.TimeForfeit;
            ShouldClaimTimeForfeitCB.Location = new Point(60, ShouldUseEtiquetteCB.Bottom + 20);
            ShouldClaimTimeForfeitCB.ForeColor = color;
            ShouldClaimTimeForfeitCB.Cursor = Cursors.Hand;
            EtiquettePanel.Controls.Add(ShouldClaimTimeForfeitCB);

            #endregion

            #region RatedGames
            Panel RatedGamesPanel = new Panel();
            RatedGamesPanel.Location = new Point(35, EtiquettePanel.Bottom + 30);
            RatedGamesPanel.Size = new System.Drawing.Size(AppearPanel.Width - 70, 150);
            RectListB.Add(RatedGamesPanel);
            BehavePanel.Tag = RectListB;
            BehavePanel.Controls.Add(RatedGamesPanel);

            ComboBox StrictnessLevelCB = new ComboBox();
            S_ControlList.Add(StrictnessLevelCB);
            StrictnessLevelCB.Name = "StrictnessLevelCB";
            Label StrictnessLevelLabel = new Label();
            StrictnessLevelLabel.Text = "Strictness level for Rated games";
            StrictnessLevelCB.ForeColor = color;
            StrictnessLevelCB.FlatStyle = FlatStyle.Flat;
            StrictnessLevelLabel.ForeColor = color;
            StrictnessLevelLabel.Font = font2;
            StrictnessLevelLabel.AutoSize = true;
            StrictnessLevelLabel.Location = new Point(30, 20);
            StrictnessLevelCB.Location = new Point(StrictnessLevelLabel.Right - 20, StrictnessLevelLabel.Bottom + 3);
            StrictnessLevelCB.Width = 290;
            StrictnessLevelCB.Items.Add("Allow takebacks and arbitrary quitting");
            StrictnessLevelCB.Items.Add("Allow arbitrary quitting only");
            StrictnessLevelCB.Items.Add("No takebacks or arbitrary quitting");
            StrictnessLevelCB.SelectedItem = settings.behaviour.RatedGamesStrictnessLevel;
            StrictnessLevelCB.Font = font2;
            StrictnessLevelCB.DropDownStyle = ComboBoxStyle.DropDownList;
            StrictnessLevelCB.Cursor = Cursors.Hand;
            RatedGamesPanel.Controls.Add(StrictnessLevelCB);
            RatedGamesPanel.Controls.Add(StrictnessLevelLabel);

            CheckBox AllowTimeChangesCB = new CheckBox();
            S_ControlList.Add(AllowTimeChangesCB);
            AllowTimeChangesCB.Font = font2;
            AllowTimeChangesCB.Text = "Allow time adjustments during rated games";
            AllowTimeChangesCB.AutoSize = true;
            AllowTimeChangesCB.Checked = settings.behaviour.AllowTimeChangesDuringGame;
            AllowTimeChangesCB.Location = new Point(StrictnessLevelLabel.Left, StrictnessLevelCB.Bottom + 30);
            AllowTimeChangesCB.ForeColor = color;
            AllowTimeChangesCB.Cursor = Cursors.Hand;
            RatedGamesPanel.Controls.Add(AllowTimeChangesCB);

            #endregion

            #endregion

            #region EnginePanel
            Panel EnginePanel = new Panel();
            EnginePanel.Location = new Point(panel.Right, 0);
            EnginePanel.AutoScroll = true;
            EnginePanel.Size = new System.Drawing.Size(483, SettingsForm.Height - 100);
            EngineLabel.Tag = EnginePanel;
            EnginePanel.BackColor = Color.LightGray;
            EnginePanel.Paint += SettingPanel_Paint;
            List<Panel> RectListE = new List<Panel>();

            CheckBox ThinkWhileIdleCB = new CheckBox();
            S_ControlList.Add(ThinkWhileIdleCB);
            ThinkWhileIdleCB.Font = font2;
            ThinkWhileIdleCB.Text = "Think while idle";
            ThinkWhileIdleCB.AutoSize = true;
            ThinkWhileIdleCB.Checked = settings.engine.Ponder;
            ThinkWhileIdleCB.Location = new Point(70, 50);
            ThinkWhileIdleCB.ForeColor = color;
            ThinkWhileIdleCB.Cursor = Cursors.Hand;
            EnginePanel.Controls.Add(ThinkWhileIdleCB);

            ComboBox OpeningBookUseCB = new ComboBox();
            S_ControlList.Add(OpeningBookUseCB);
            OpeningBookUseCB.Name = "OpeningBookUseCB";
            Label OpeningBookUseLabel = new Label();
            OpeningBookUseLabel.Text = "Opening book use";
            OpeningBookUseCB.ForeColor = color;
            OpeningBookUseCB.FlatStyle = FlatStyle.Flat;
            OpeningBookUseLabel.ForeColor = color;
            OpeningBookUseLabel.Font = font2;
            OpeningBookUseLabel.AutoSize = true;
            OpeningBookUseLabel.Location = new Point(30, ThinkWhileIdleCB.Bottom + 20);
            OpeningBookUseCB.Location = new Point(OpeningBookUseLabel.Right + 50, OpeningBookUseLabel.Top - 3);
            OpeningBookUseCB.Width = 230;
            OpeningBookUseCB.Items.Add("Always use Default Book");
            OpeningBookUseCB.Items.Add("Use engine book if available");
            OpeningBookUseCB.SelectedItem = settings.engine.OpeningBookUse;
            OpeningBookUseCB.Font = font2;
            OpeningBookUseCB.DropDownStyle = ComboBoxStyle.DropDownList;
            OpeningBookUseCB.Cursor = Cursors.Hand;
            EnginePanel.Controls.Add(OpeningBookUseCB);
            EnginePanel.Controls.Add(OpeningBookUseLabel);

            ComboBox HashSizeCB = new ComboBox();
            S_ControlList.Add(HashSizeCB);
            HashSizeCB.Name = "HashSizeCB";
            Label HashSizeLabel = new Label();
            HashSizeLabel.Text = "Hash memory size";
            HashSizeCB.ForeColor = color;
            HashSizeCB.FlatStyle = FlatStyle.Flat;
            HashSizeLabel.ForeColor = color;
            HashSizeLabel.Font = font2;
            HashSizeLabel.AutoSize = true;
            HashSizeLabel.Location = new Point(30, OpeningBookUseCB.Bottom + 30);
            HashSizeCB.Location = new Point(HashSizeLabel.Right + 50, HashSizeLabel.Top - 3);
            HashSizeCB.Width = 210;
            HashSizeCB.Items.Add("Always ask");
            HashSizeCB.Items.Add("Always use engine default");
            HashSizeCB.Items.Add("Use a general number");
            HashSizeCB.Items.Add("Specify for each engine. . .");
            int a;
            HashSizeCB.SelectedItem = int.TryParse(settings.engine.HashSizeHandling, out a) ? 
                "Use a general number" : settings.engine.HashSizeHandling;
            HashSizeCB.Font = font2;
            HashSizeCB.SelectedValueChanged += HashSelectedValueChanged;
            HashSizeCB.DropDownClosed += HashDropDownClosed;
            HashSizeCB.DropDown += HashSizeCB_DropDown;
            HashSizeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            HashSizeCB.Cursor = Cursors.Hand;
            TextBox tb = new TextBox();
            Label lb = new Label();
            lb.Text = "MB";
            lb.ForeColor = color;
            tb.ForeColor = color;
            lb.AutoSize = true;
            tb.Width = 45;
            tb.Font = font2;
            tb.Text = int.TryParse(settings.engine.HashSizeHandling, out a) ? a.ToString() : "64";
            lb.Font = new Font(font2.Name, font2.Size + 1, font2.Style);
            tb.Location = new Point(HashSizeCB.Right + 10, HashSizeCB.Top);
            lb.Location = new Point(tb.Right, tb.Top);
            tb.Visible = (string)HashSizeCB.SelectedItem == "Use a general number";
            lb.Visible = tb.Visible;
            HashSizeCB.Tag = Tuple.Create(tb, lb);

            EnginePanel.Controls.Add(tb);
            EnginePanel.Controls.Add(lb);
            EnginePanel.Controls.Add(HashSizeCB);
            EnginePanel.Controls.Add(HashSizeLabel);

            ComboBox ThreadCountCB = new ComboBox();
            S_ControlList.Add(ThreadCountCB);
            ThreadCountCB.Name = "ThreadCountCB";
            Label ThreadCountLabel = new Label();
            ThreadCountLabel.Text = "CPU cores for engine use";
            ThreadCountCB.ForeColor = color;
            ThreadCountCB.FlatStyle = FlatStyle.Flat;
            ThreadCountLabel.ForeColor = color;
            ThreadCountLabel.Font = font2;
            ThreadCountLabel.AutoSize = true;
            ThreadCountLabel.Location = new Point(30, HashSizeCB.Bottom + 30);
            ThreadCountCB.Location = new Point(ThreadCountLabel.Right + 100, ThreadCountLabel.Top - 3);
            ThreadCountCB.Width = 160;
            ThreadCountCB.Items.Add("Engine Default");
            for (int i = 1; i <= Environment.ProcessorCount; i++)
                ThreadCountCB.Items.Add(i.ToString());
            ThreadCountCB.SelectedItem = settings.engine.ThreadCount;
            ThreadCountCB.Font = font2;
            ThreadCountCB.DropDownStyle = ComboBoxStyle.DropDownList;
            ThreadCountCB.Cursor = Cursors.Hand;
            EnginePanel.Controls.Add(ThreadCountCB);
            EnginePanel.Controls.Add(ThreadCountLabel);

            #endregion

            #region MiscPanel
            Panel MiscPanel = new Panel();
            MiscPanel.Location = new Point(panel.Right, 0);
            MiscPanel.BackColor = Color.LightGray;
            MiscPanel.AutoScroll = true;
            MiscPanel.Size = new System.Drawing.Size(483, SettingsForm.Height - 100);
            MiscLabel.Tag = MiscPanel;
            MiscPanel.Paint += SettingPanel_Paint;
            List<Panel> RectListM = new List<Panel>();

            ComboBox AutosaveforRatedCB = new ComboBox();
            S_ControlList.Add(AutosaveforRatedCB);
            AutosaveforRatedCB.Name = "AutosaveforRatedCB";
            Label AutosaveforRatedLabel = new Label();
            AutosaveforRatedLabel.Text = "Autosave (rated games)";
            AutosaveforRatedCB.ForeColor = color;
            AutosaveforRatedCB.FlatStyle = FlatStyle.Flat;
            AutosaveforRatedLabel.ForeColor = color;
            AutosaveforRatedLabel.Font = font2;
            AutosaveforRatedLabel.AutoSize = true;
            AutosaveforRatedLabel.Location = new Point(60, 50);
            AutosaveforRatedCB.Location = new Point(AutosaveforRatedLabel.Right + 110, AutosaveforRatedLabel.Top - 3);
            AutosaveforRatedCB.Width = 150;
            AutosaveforRatedCB.Items.Add("Always ask");
            AutosaveforRatedCB.Items.Add("After each game");
            AutosaveforRatedCB.Items.Add("After each move");
            AutosaveforRatedCB.Items.Add("Don't save");
            AutosaveforRatedCB.SelectedItem = settings.miscellaneous.AutoSaveForRatedGames;
            AutosaveforRatedCB.Font = font2;
            AutosaveforRatedCB.DropDownStyle = ComboBoxStyle.DropDownList;
            AutosaveforRatedCB.Cursor = Cursors.Hand;
            MiscPanel.Controls.Add(AutosaveforRatedCB);
            MiscPanel.Controls.Add(AutosaveforRatedLabel);

            ComboBox AutosaveforNonRatedCB = new ComboBox();
            S_ControlList.Add(AutosaveforNonRatedCB);
            AutosaveforNonRatedCB.Name = "AutosaveforNonRatedCB";
            Label AutosaveforNonRatedLabel = new Label();
            AutosaveforNonRatedLabel.Text = "Autosave (non-rated games)";
            AutosaveforNonRatedCB.ForeColor = color;
            AutosaveforNonRatedCB.FlatStyle = FlatStyle.Flat;
            AutosaveforNonRatedLabel.ForeColor = color;
            AutosaveforNonRatedLabel.Font = font2;
            AutosaveforNonRatedLabel.AutoSize = true;
            AutosaveforNonRatedLabel.Location = new Point(60, AutosaveforRatedCB.Bottom + 30);
            AutosaveforNonRatedCB.Location = new Point(AutosaveforNonRatedLabel.Right + 110, AutosaveforNonRatedLabel.Top - 3);
            AutosaveforNonRatedCB.Width = 150;
            AutosaveforNonRatedCB.Items.Add("Always ask");
            AutosaveforNonRatedCB.Items.Add("After each game");
            AutosaveforNonRatedCB.Items.Add("After each move");
            AutosaveforNonRatedCB.Items.Add("Don't save");
            AutosaveforNonRatedCB.SelectedItem = settings.miscellaneous.AutoSaveForNonRatedGames;
            AutosaveforNonRatedCB.Font = font2;
            AutosaveforNonRatedCB.DropDownStyle = ComboBoxStyle.DropDownList;
            AutosaveforNonRatedCB.Cursor = Cursors.Hand;
            MiscPanel.Controls.Add(AutosaveforNonRatedCB);
            MiscPanel.Controls.Add(AutosaveforNonRatedLabel);

            #region Sounds
            Panel SoundPanel = new Panel();
            SoundPanel.Location = new Point(35, AutosaveforNonRatedCB.Bottom + 30);
            SoundPanel.Size = new System.Drawing.Size(MiscPanel.Width - 70, 100);
            RectListM.Add(SoundPanel);
            MiscPanel.Tag = RectListM;
            MiscPanel.Controls.Add(SoundPanel);

            CheckBox ShouldPlaySoundsCB = new CheckBox();
            S_ControlList.Add(ShouldPlaySoundsCB);
            ShouldPlaySoundsCB.Font = font2;
            ShouldPlaySoundsCB.Text = "Play sounds";
            ShouldPlaySoundsCB.AutoSize = true;
            ShouldPlaySoundsCB.Checked = settings.miscellaneous.ShouldPlaySounds;
            ShouldPlaySoundsCB.Location = new Point(60, 10);
            ShouldPlaySoundsCB.ForeColor = color;
            ShouldPlaySoundsCB.Cursor = Cursors.Hand;
            ShouldPlaySoundsCB.CheckedChanged += SettingsCheckBox_CheckedChanged;
            SoundPanel.Controls.Add(ShouldPlaySoundsCB);

            ComboBox MoveSoundThemeCB = new ComboBox();
            S_ControlList.Add(MoveSoundThemeCB);
            MoveSoundThemeCB.Name = "MoveSoundThemeCB";
            Label MoveSoundThemeLabel = new Label();
            MoveSoundThemeLabel.Text = "Move sound theme";
            MoveSoundThemeCB.ForeColor = color;
            MoveSoundThemeCB.FlatStyle = FlatStyle.Flat;
            MoveSoundThemeLabel.ForeColor = color;
            MoveSoundThemeLabel.Font = font2;
            MoveSoundThemeLabel.AutoSize = true;
            MoveSoundThemeLabel.Location = new Point(80, ShouldPlaySoundsCB.Bottom + 20);
            MoveSoundThemeCB.Location = new Point(MoveSoundThemeLabel.Right + 50, MoveSoundThemeLabel.Top - 3);
            MoveSoundThemeCB.Width = 150;
            MoveSoundThemeCB.Enabled = ShouldPlaySoundsCB.Checked;
            MoveSoundThemeLabel.Enabled = ShouldPlaySoundsCB.Checked;
            MoveSoundThemeCB.Items.Add("Classic");
            MoveSoundThemeCB.Items.Add("Robot");
            MoveSoundThemeCB.Items.Add("Piano");
            MoveSoundThemeCB.Items.Add("Speech");
            MoveSoundThemeCB.SelectedItem = settings.miscellaneous.MoveSoundTheme;
            MoveSoundThemeCB.Font = font2;
            MoveSoundThemeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            MoveSoundThemeCB.Cursor = Cursors.Hand;
            List<Control> list4 = new List<Control>();
            list4.Add(MoveSoundThemeCB);
            list4.Add(MoveSoundThemeLabel);
            ShouldPlaySoundsCB.Tag = list4;
            SoundPanel.Controls.Add(MoveSoundThemeCB);
            SoundPanel.Controls.Add(MoveSoundThemeLabel);

            #endregion

            #endregion

            #region Buttons
            Button Save = new Button();
            Button Cancel = new Button();
            Button Default = new Button();
            Save.Location = new Point(590, 510);
            Cancel.Location = new Point(460, 510);
            Default.Location = new Point(250, 514);
            Save.Text = "&Save";
            Cancel.Text = "&Cancel";
            Default.Text = "Restore &Defaults";
            Save.Click += SettingsButton_Click;
            Cancel.Click += SettingsButton_Click;
            Default.Click += Default_Click;
            Save.Cursor = Cursors.Hand;
            Cancel.Cursor = Cursors.Hand;
            Default.Cursor = Cursors.Hand;
            Save.BackColor = Color.DarkGray;
            Cancel.BackColor = Color.DarkGray;
            Default.BackColor = Color.DarkGray;
            Save.Font = new System.Drawing.Font("Segoe UI", 18, FontStyle.Bold);
            Cancel.Font = new System.Drawing.Font("Segoe UI", 17, FontStyle.Bold);
            Default.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            Save.AutoSize = true;
            Cancel.AutoSize = true;
            Default.AutoSize = true;
            SettingsForm.Controls.Add(Save);
            SettingsForm.Controls.Add(Default);
            SettingsForm.Controls.Add(Cancel);
            #endregion

            panel.Controls.Add(AppearLabel);
            panel.Controls.Add(BehaveLabel);
            panel.Controls.Add(EngineLabel);
            panel.Controls.Add(MiscLabel);
            SettingsForm.Tag = Tuple.Create(AppearLabel, S_ControlList);
            SettingLabel_Click(AppearLabel, new EventArgs());
            SettingsForm.Controls.Add(panel);
            SettingsForm.ShowDialog(this);

            #region Las Las
            if (SettingsForm.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                // Changing engine settings while engine is playing a move

                settings.appearance.AnimationType = AnimationCB.SelectedItem.ToString();
                settings.appearance.BoardCordinatesSides = BoardCordinatesCB.SelectedItem.ToString();
                settings.appearance.ChessFont = (ChessFont)PieceSetCB.SelectedItem;
                settings.appearance.ColorTheme = (PresetTheme.Theme)ColorThemeCB.SelectedItem;
                settings.appearance.KICHLType = KICHLTypeCB.SelectedItem.ToString();
                settings.appearance.LastMoveHLType = LastMoveTypeCB.SelectedItem.ToString();
                settings.appearance.SaveUserArrows = UserArrowSaveCB.SelectedItem.ToString();
                settings.appearance.ShouldShowBMA = ShouldShowBMACB.Checked;
                settings.appearance.ShouldShowKICHL = ShouldShowKICHLCB.Checked;
                settings.appearance.ShouldShowLastMoveHL = ShouldShowLastMoveCB.Checked;
                settings.appearance.ShouldShowLegalMovesHL = ShouldShowLegalMovesCB.Checked;
                settings.appearance.ShouldShowUserArrows = ShouldShowUserArrowCB.Checked;
                settings.appearance.UserArrowSquareHLType = UserArrowTypeCB.SelectedItem.ToString();

                settings.behaviour.AllowTimeChangesDuringGame = AllowTimeChangesCB.Checked;
                settings.behaviour.NewMoveHandling = NewMoveCB.SelectedItem.ToString();
                settings.behaviour.PromotionType = PromotionCB.SelectedItem.ToString();
                settings.behaviour.RatedGamesStrictnessLevel = StrictnessLevelCB.SelectedItem.ToString();
                settings.behaviour.ResignAndOfferDraw = ShouldUseEtiquetteCB.Checked;
                settings.behaviour.TimeForfeit = ShouldClaimTimeForfeitCB.Checked;

                settings.engine.HashSizeHandling = (string)HashSizeCB.SelectedItem == "Use a general number" ?
                    (int.TryParse(tb.Text, out a) ? tb.Text : "64") : HashSizeCB.SelectedItem.ToString();
                settings.engine.ThreadCount = (string)ThreadCountCB.SelectedItem;
                // include support for Ponder setting
                foreach (var item in LoadedEngines)
                {
                    if (!item._Threads)
                        continue;
                    EngineOption opt = null;
                    foreach (var option in item.Options)
                        if (option.Name == "Threads")
	                    {
                            opt = option;
                            break;
	                    }
                    if (item.isAnalyzing)
                    {
                        item.Process.StandardInput.WriteLine("stop");
                    }

                    item.Process.StandardInput.WriteLine("setoption name Threads value " +
                        ((string)ThreadCountCB.SelectedItem != "Engine Default" ?
                        ThreadCountCB.SelectedItem : opt.DefaultValue.ToString()));
                    if (!WaitOutEngine(item, 5, false))
                        return;
                }
                if (int.TryParse(settings.engine.HashSizeHandling, out a))
                {
                    foreach (var item in LoadedEngines)
                        if (item._Hash)
                        {
                            item.Process.StandardInput.WriteLine("setoption name Hash value " + a);
                            if (!WaitOutEngine(item, 5, false))
                                return;
                        }
                }
                else
                {
                    if (settings.engine.HashSizeHandling == "Always use engine default")
                    {
                        foreach (var item in LoadedEngines)
                            {
                                if (!item._Hash)
                                    continue;
                                EngineOption opt = null;
                                foreach (var option in item.Options)
                                    if (option.Name == "Hash")
                                    {
                                        opt = option;
                                        break;
                                    }
                                item.Process.StandardInput.WriteLine("setoption name Hash value " +
                                    opt.DefaultValue.ToString());
                                if (!WaitOutEngine(item, 5, false))
                                    return;
                            }
                        }
                    else if (settings.engine.HashSizeHandling == "Specify for each engine. . .")
                    {
                        foreach (var item in settings.engine.HashOptionList)
                        {
                            if (LoadedEngines.Contains(item.Item1))
                            {
                                if (!item.Item1._Hash)
                                    continue;
                                if (int.TryParse(item.Item2, out a))
                                {
                                    item.Item1.Process.StandardInput.WriteLine("setoption name Hash value " + a);
                                    if (!WaitOutEngine(item.Item1, 5, false))
                                        return;
                                }
                                else if (item.Item2 == "Use engine default")
                                {
                                    EngineOption opt = null;
                                    foreach (var option in item.Item1.Options)
                                        if (option.Name == "Hash")
                                        {
                                            opt = option;
                                            break;
                                        }
                                    item.Item1.Process.StandardInput.WriteLine("setoption name Hash value " +
                                        opt.DefaultValue.ToString());
                                    if (!WaitOutEngine(item.Item1, 5, false))
                                        return;
                                }
                            }
                        }
                    }
                }
                foreach (var item in LoadedEngines)
                    if (item.isAnalyzing)
                        item.Process.StandardInput.WriteLine("go infinite");
                settings.engine.OpeningBookUse = OpeningBookUseCB.SelectedItem.ToString();
                settings.engine.Ponder = ThinkWhileIdleCB.Checked;

                settings.miscellaneous.AutoSaveForNonRatedGames = AutosaveforNonRatedCB.SelectedItem.ToString();
                settings.miscellaneous.AutoSaveForRatedGames = AutosaveforRatedCB.SelectedItem.ToString();
                settings.miscellaneous.MoveSoundTheme = MoveSoundThemeCB.SelectedItem.ToString();
                settings.miscellaneous.ShouldPlaySounds = ShouldPlaySoundsCB.Checked;

                ColorTheme = new PresetTheme(settings.appearance.ColorTheme);
                Form1.LightSquareColor = ColorTheme.LightSquareColor;
                Form1.DarkSquareColor = ColorTheme.DarkSquareColor;
                Form1.ShiftColor = ColorTheme.ShiftColor;
                Form1.CtrlColor = ColorTheme.CtrlColor;
                Form1.AltColor = ColorTheme.AltColor;
                Form1.LastMoveHLColor = ColorTheme.LastMoveColor;
                Form1.LegalMoveHLColor = ColorTheme.LegalMHLColor;
                Form1.BMAColor = ColorTheme.BMAColor;
                BestMoveArrow.Pen.Color = Form1.BMAColor;

                // PieceSet implementation
                ShouldAnimate = settings.appearance.AnimationType != "None";
                InitializeSquareCordinates();
                ShouldHighlightLastMove = settings.appearance.ShouldShowLastMoveHL;
                ShouldHighlightCheckedKing = settings.appearance.ShouldShowKICHL;
                ShouldDrawUserArrows = settings.appearance.ShouldShowUserArrows;
                ShouldHLLegalSquares = settings.appearance.ShouldShowLegalMovesHL;
                ShouldAnimate = settings.appearance.AnimationType != "None";
                if (settings.appearance.AnimationType == "Fast")
                    AnimationTimer.Interval = 30;
                else if (settings.appearance.AnimationType == "Slow")
                    AnimationTimer.Interval = 50;

                ShouldUseEtiquette = settings.behaviour.ResignAndOfferDraw;

                
                RefreshSquares(Squares, RedrawPerspective.None, null);
            }
            #endregion
        }
        private void HashDropDownClosed(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ShowHashSizeSetting(sender, new EventArgs());
            cb.SelectedValueChanged += HashSelectedValueChanged;
        }
        private void HashSelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            (cb.Tag as Tuple<TextBox, Label>).Item1.Visible = (string)cb.SelectedItem == "Use a general number";
            (cb.Tag as Tuple<TextBox, Label>).Item2.Visible = (string)cb.SelectedItem == "Use a general number";

            ShowHashSizeSetting(sender, new EventArgs());
        }
        void HashSizeCB_DropDown(object sender, EventArgs e)
        {
            (sender as ComboBox).SelectedValueChanged -= HashSelectedValueChanged;
        }
        void ShowHashSizeSetting(object sender, EventArgs e)
        {
            if ((string)(sender as ComboBox).SelectedItem != "Specify for each engine. . .")
                return;
            if (HashSettingFormDelay)
                return;


            HashOptionsForm = new Form();
            HashOptionsForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            HashOptionsForm.Text = "Specify Hash size for each engine";
            HashOptionsForm.MinimizeBox = false;
            HashOptionsForm.MaximizeBox = false;
            HashOptionsForm.AutoScroll = true;
            HashOptionsForm.BackColor = Color.LightGray;
            HashOptionsForm.StartPosition = FormStartPosition.CenterParent;
            HashOptionsForm.Size = new Size(600, 500);

            Font font = new Font("Segoe UI", 12, FontStyle.Regular);
            Color color = Color.DarkRed;
            Label last = null;

            foreach (var engine in InstalledEngines)
            {
                bool EXISTS = false;
                foreach (var item in settings.engine.HashOptionList)
                    if (engine.Equals(item.Item1))
                    {
                        EXISTS = true;
                        break;
                    }
                if (!EXISTS)
                    settings.engine.HashOptionList.Add(Tuple.Create(engine, "Always ask"));
            }

            foreach (var item in settings.engine.HashOptionList)
            {
                Label label = new Label();
                label.Text = item.Item1.Name;
                label.Font = font;
                label.Location = new Point(30, last == null ? 50 : last.Bottom + 20);
                label.AutoSize = true;
                label.ForeColor = color;
                last = label;

                ComboBox cb = new ComboBox();
                cb.Font = font;
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.FlatStyle = FlatStyle.Flat;
                cb.Items.Add("Always ask");
                cb.Items.Add("Use engine default");
                cb.Items.Add("Use specific number");
                int a;
                if (int.TryParse(item.Item2, out a))
                    cb.SelectedItem = "Use specific number";
                cb.SelectedItem = item.Item2;
                cb.Width += 50;
                label.Tag = cb;
                cb.ForeColor = color;
                cb.Cursor = Cursors.Hand;
                cb.Location = new Point(label.Right + 100, label.Top - 3);
                cb.SelectedValueChanged += Hashcb_SelectedValueChanged;

                TextBox tb = new TextBox();
                Label lb = new Label();
                lb.Text = "MB";
                lb.Font = new Font(font.Name, 14, font.Style);
                int x;
                tb.Width = 50;
                tb.ForeColor = color;
                lb.ForeColor = color;
                tb.Font = font;
                tb.Text = int.TryParse(item.Item2, out x) ? x.ToString() : "64";
                tb.Location = new Point(cb.Right + 30, label.Top);
                lb.Location = new Point(tb.Right, tb.Top);
                lb.AutoSize = true;
                lb.Visible = (string)cb.SelectedItem == "Use specific number";
                tb.Visible = lb.Visible;
                var list = new List<Control>();
                list.Add(tb);
                list.Add(lb);
                cb.Tag = list;

                HashOptionsForm.Controls.Add(label);
                HashOptionsForm.Controls.Add(cb);
                HashOptionsForm.Controls.Add(tb);
                HashOptionsForm.Controls.Add(lb);
            }

            Button OK = new Button(), Cancel = new Button();
            OK.Text = "OK";
            Cancel.Text = "Cancel";
            OK.BackColor = Color.Gray;
            Cancel.BackColor = OK.BackColor;
            OK.Font = new System.Drawing.Font(font.Name, 16, FontStyle.Bold);
            Cancel.Font = OK.Font;
            OK.AutoSize = true;
            OK.Click += HashButton_Click;
            Cancel.Click += HashButton_Click;
            Cancel.AutoSize = true;
            Cancel.Location = new Point(150, last.Bottom + 100);
            OK.Location = new Point(350, last.Bottom + 100);
            HashOptionsForm.Controls.Add(OK);
            HashOptionsForm.Controls.Add(Cancel);

            HashOptionsForm.ShowDialog(SettingsForm);
            if (HashOptionsForm.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                settings.engine.HashOptionList.Clear();
                foreach (var engine in InstalledEngines)
                {
                    foreach (var item in HashOptionsForm.Controls)
                    {
                        if (item is Label && (item as Label).Text == engine.Name)
                        {
                            Label l = item as Label;
                            if ((string)(l.Tag as ComboBox).SelectedItem == "Use specific number")
                            {
                                int x;
                                if (int.TryParse(((l.Tag as ComboBox).Tag as List<Control>)[0].Text, out x))
                                    settings.engine.HashOptionList.Add(Tuple.Create(engine, x.ToString()));
                                else
                                    settings.engine.HashOptionList.Add(Tuple.Create(engine, "64"));
                            }
                            else
                                settings.engine.HashOptionList.Add(Tuple.Create(engine, 
                                    (l.Tag as ComboBox).SelectedItem.ToString()));
                        }
                    }
                }
            }
            HashSettingFormDelay = true;
            var timer = new Timer();
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Tick += hashTimer_Tick;
            timer.Start();
        }
        void hashTimer_Tick(object sender, EventArgs e)
        {
            HashSettingFormDelay = false;
            (sender as Timer).Stop();
        }
        void HashButton_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "OK")
                HashOptionsForm.DialogResult = System.Windows.Forms.DialogResult.OK;
            else if ((sender as Button).Text == "Cancel")
                HashOptionsForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            HashOptionsForm.Close();
        }
        private void Hashcb_SelectedValueChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            foreach (var item in cb.Tag as List<Control>)
            {
                if ((string)cb.SelectedItem == "Use specific number")
                {
                    item.Visible = true;
                    if (item is TextBox)
                        item.Focus();
                }
            }
        }
        void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                SettingsForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                SettingsForm.Close();
            }
            if (e.KeyCode == Keys.Enter)
            {
                SettingsForm.DialogResult = System.Windows.Forms.DialogResult.OK;
                SettingsForm.Close();
            }
        }
        void Default_Click(object sender, EventArgs e)
        {
            settings = new Settings();
            foreach (var item in (SettingsForm.Tag as Tuple<Label, List<Control>>).Item2)
            {
                if (item is CheckBox)
                {
                    CheckBox cb = item as CheckBox;
                    if (cb.Text == "Show Legal Moves")
                    {
                        cb.Checked = settings.appearance.ShouldShowLegalMovesHL;
                        continue;
                    }
                    if (cb.Text == "Highlight last move played")
                    {
                        cb.Checked = settings.appearance.ShouldShowLastMoveHL;
                        continue;
                    }
                    if (cb.Text == "Highlight King-in-check")
                    {
                        cb.Checked = settings.appearance.ShouldShowKICHL;
                        continue;
                    }
                    if (cb.Text == "Show arrows drawn by user")
                    {
                        cb.Checked = settings.appearance.ShouldShowUserArrows;
                        continue;
                    }
                    if (cb.Text == "Show best-move arrow (during analysis)")
                    {
                        cb.Checked = settings.appearance.ShouldShowBMA;
                        continue;
                    }
                    if (cb.Text == "Resign and offer draw")
                    {
                        cb.Checked = settings.behaviour.ResignAndOfferDraw;
                        continue;
                    }
                    if (cb.Text == "Claim forfeit on time")
                    {
                        cb.Checked = settings.behaviour.TimeForfeit;
                        continue;
                    }
                    if (cb.Text == "Allow time adjustments during rated games")
                    {
                        cb.Checked = settings.behaviour.AllowTimeChangesDuringGame;
                        continue;
                    }
                    if (cb.Text == "Think while idle")
                    {
                        cb.Checked = settings.engine.Ponder;
                        continue;
                    }
                    if (cb.Text == "Play sounds")
                    {
                        cb.Checked = settings.miscellaneous.ShouldPlaySounds;
                        continue;
                    }
                }
                else if (item is ComboBox)
                {
                    ComboBox cb = item as ComboBox;
                    if (cb.Name == "PieceSetCB")
                    {
                        cb.SelectedItem = settings.appearance.ChessFont;
                        continue;
                    }
                    if (cb.Name == "ColorThemeCB")
                    {
                        cb.SelectedItem = settings.appearance.ColorTheme;
                        continue;
                    }
                    if (cb.Name == "AnimationCB")
                    {
                        cb.SelectedItem = settings.appearance.AnimationType;
                        continue;
                    }
                    if (cb.Name == "LastMoveTypeCB")
                    {
                        cb.SelectedItem = settings.appearance.LastMoveHLType;
                        continue;
                    }
                    if (cb.Name == "KICHLTypeCB")
                    {
                        cb.SelectedItem = settings.appearance.KICHLType;
                        continue;
                    }
                    if (cb.Name == "UserArrowTypeCB")
                    {
                        cb.SelectedItem = settings.appearance.UserArrowSquareHLType;
                        continue;
                    }
                    if (cb.Name == "UserArrowSaveCB")
                    {
                        cb.SelectedItem = settings.appearance.SaveUserArrows;
                        continue;
                    }
                    if (cb.Name == "BoardCordinatesCB")
                    {
                        cb.SelectedItem = settings.appearance.BoardCordinatesSides;
                        continue;
                    }
                    if (cb.Name == "PromotionCB")
                    {
                        cb.SelectedItem = settings.behaviour.PromotionType;
                        continue;
                    }
                    if (cb.Name == "NewMoveCB")
                    {
                        cb.SelectedItem = settings.behaviour.NewMoveHandling;
                        continue;
                    }
                    if (cb.Name == "StrictnessLevelCB")
                    {
                        cb.SelectedItem = settings.behaviour.RatedGamesStrictnessLevel;
                        continue;
                    }
                    if (cb.Name == "OpeningBookUseCB")
                    {
                        cb.SelectedItem = settings.engine.OpeningBookUse;
                        continue;
                    }
                    if (cb.Name == "HashSizeCB")
                    {
                        cb.SelectedItem = settings.engine.HashSizeHandling;
                        continue;
                    }
                    if (cb.Name == "ThreadCountCB")
                    {
                        cb.SelectedItem = settings.engine.ThreadCount;
                        continue;
                    }
                    if (cb.Name == "AutosaveforRatedCB")
                    {
                        cb.SelectedItem = settings.miscellaneous.AutoSaveForRatedGames;
                        continue;
                    }
                    if (cb.Name == "AutosaveforNonRatedCB")
                    {
                        cb.SelectedItem = settings.miscellaneous.AutoSaveForNonRatedGames;
                        continue;
                    }
                    if (cb.Name == "MoveSoundThemeCB")
                    {
                        cb.SelectedItem = settings.miscellaneous.MoveSoundTheme;
                        continue;
                    }
                }
            }
        }
        void SettingsButton_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "&Save")
                SettingsForm.DialogResult = System.Windows.Forms.DialogResult.OK;
            else if ((sender as Button).Text == "&Cancel")
                SettingsForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            SettingsForm.Close();
        }
        private void SettingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            foreach (var item in cb.Tag as List<Control>)
            {
                item.Enabled = cb.Checked;
            }
        }
        void SettingPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel.Tag == null)
                return;

            foreach (var item in panel.Tag as List<Panel>)
            {
                Rectangle rect = new Rectangle(item.Location - new Size(2, 2), item.Size + new Size(2, 2));
                panel.CreateGraphics().DrawRectangle(new Pen(Color.DarkRed), rect);
            }
        }
        private void SettingLabel_Click(object sender, EventArgs e)
        {
            if ((SettingsForm.Tag as Tuple<Label, List<Control>>).Item1 != null)
            {
                (SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.BorderStyle = BorderStyle.None;
                (SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.ForeColor = Color.OldLace;
                (SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.Font = new Font("Segoe UI", 15, FontStyle.Bold);

                if ((SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.Tag != null)
                    SettingsForm.Controls.Remove((SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.Tag as Control);
            }
            SettingsForm.Tag = Tuple.Create(sender as Label,
                (SettingsForm.Tag as Tuple<Label, List<Control>>).Item2);
            (sender as Label).BorderStyle = BorderStyle.Fixed3D;
            (sender as Label).ForeColor = Color.OldLace;
            (sender as Label).Font = new Font("Segoe UI", 19, FontStyle.Bold);
            if ((SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.Tag != null)
                SettingsForm.Controls.Add((SettingsForm.Tag as Tuple<Label, List<Control>>).Item1.Tag as Control);
        }
        void SettingLabel_MouseLeave(object sender, EventArgs e)
        {
            if (sender == SettingsForm.Tag)
                return;
            (sender as Label).ForeColor = Color.OldLace;
        }
        void SettingLabel_MouseEnter(object sender, EventArgs e)
        {
            if (sender == (SettingsForm.Tag as Tuple<Label, List<Control>>).Item1)
                return;
            (sender as Label).ForeColor = Color.DimGray;
            //(sender as Label).Font = new Font("Segoe UI", 19, FontStyle.Bold);
        }
        private void InitializeSettings()
        {
            try
            {
                if (File.Exists("Data\\settings.dat"))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (Stream input = File.OpenRead("Data\\settings.dat"))
                        settings = bf.Deserialize(input) as Settings;
                }
            }
            catch (Exception)
            {
                settings = null;
            }

            if (settings == null)
            {
                settings = new Settings();
            }

            ColorTheme = new PresetTheme(settings.appearance.ColorTheme);
            Form1.LightSquareColor = ColorTheme.LightSquareColor;
            Form1.DarkSquareColor = ColorTheme.DarkSquareColor;
            Form1.ShiftColor = ColorTheme.ShiftColor;
            Form1.CtrlColor = ColorTheme.CtrlColor;
            Form1.AltColor = ColorTheme.AltColor;
            Form1.LastMoveHLColor = ColorTheme.LastMoveColor;
            Form1.LegalMoveHLColor = ColorTheme.LegalMHLColor;
            Form1.BMAColor = ColorTheme.BMAColor;

            // PieceSet implementation

            ShouldAnimate = settings.appearance.AnimationType != "None";
            ShouldHighlightLastMove = settings.appearance.ShouldShowLastMoveHL;
            ShouldHighlightCheckedKing = settings.appearance.ShouldShowKICHL;
            ShouldDrawUserArrows = settings.appearance.ShouldShowUserArrows;
            ShouldHLLegalSquares = settings.appearance.ShouldShowLegalMovesHL;

            ShouldUseEtiquette = settings.behaviour.ResignAndOfferDraw;
        }
        private void InitializeState()
        {
            try
            {
                if (File.Exists("Data\\state.dat"))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (Stream input = File.OpenRead("Data\\state.dat"))
                        state = bf.Deserialize(input) as State;
                }
            }
            catch (Exception)
            {
                state = null;
            }

            if (state == null)
            {
                state = new State();
            }

            timeControl = state.TimeControl;
            BlackTime = state.BlackTime;
            WhiteTime = state.WhiteTime;
            WhiteTime.ClockLabel = WhiteClockLabel;
            BlackTime.ClockLabel = BlackClockLabel;

            CurrentUser = state.CurrentUser;
            UserList = state.UserList;

            Squares = state.Squares;
            StartingPosition = state.StartingPosition;
            FENforStartPos = state.FENForStartPosition;
            TwoDSquares = state.TwoDSquares;
            if (Squares != null)
            {
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
            }

            isEngineMatchInProgress = false;    //  = state.isEngineMatchInProgress;  // stubbing out . . . for now
            isRatedGameInProgress = state.isRatedGameInProgress;
            ShouldClockTick = state.ShouldClockTick;
            CurrentPlayer = state.CurrentPlayer;
            ModeOfPlay = state.ModeOfPlay;

            FirstEngine = state.FirstEngine;
            SecondEngine = state.SecondEngine;

            if (state.Game != null)
            {
                if (state.Game.CurrentVariation != null)
                    CurrentVariation = state.Game.CurrentVariation;
                if (state.Game.GameVariations != null)
                    GameVariations = state.Game.GameVariations;
                if (state.Game.GameDetails != null)
                    gameDetails = state.Game.GameDetails;
                if (state.Game.MainLine != null)
                    MainLine = state.Game.MainLine;
                if (state.Game.Pieces != null)
                    Pieces = state.Game.Pieces;
                if (state.Game.CapturedPieces != null)
                    CapturedPieces = state.Game.CapturedPieces;
            }
        }
        private void LoadInitPosition()
        {
            foreach (var item in CurrentPosition.PieceInfos)
            {
                PlacePiece(item.Piece, item.Square, false);
            }

            CheckingPiece = CurrentPosition.CheckingPiece;
            DoubleCheckingPiece = CurrentPosition.DoubleCheckingPiece;
            IsBlackCheckmated = CurrentPosition.IsBlackCheckmated;
            IsBlackInCheck = CurrentPosition.IsBlackInCheck;
            IsDraw = CurrentPosition.IsDraw;
            IsWhiteCheckmated = CurrentPosition.IsWhiteCheckmated;
            IsWhiteInCheck = CurrentPosition.IsWhiteInCheck;
            KingsideCastlingBlack = CurrentPosition.KingsideCastlingBlack;
            KingsideCastlingWhite = CurrentPosition.KingsideCastlingWhite;
            QueensideCastlingBlack = CurrentPosition.QueensideCastlingBlack;
            QueensideCastlingWhite = CurrentPosition.QueensideCastlingWhite;
            EnPassantPawn = CurrentPosition.EnPassantPawn;
            sideToPlay = CurrentPosition.sideToPlay;
            FiftyMoveCount = CurrentPosition.FiftyMoveCount;
            PossibleDefences = CurrentPosition.PossibleDefences;
            MoveCount = CurrentPosition.MoveCount;

            foreach (var item in Squares)
            {
                if (item.Piece != null)
                {
                    if (item.Piece.Name == "White King")
                        WhiteKing = item.Piece;
                    else if (item.Piece.Name == "Black King")
                        BlackKing = item.Piece;
                }
            }

            isBoardFlipped = state.isBoardFlipped;

            HighlightCheckedKing();
            if (CurrentPosition.LastMovePlayed != null)
            {
                HighLightLastMove(CurrentPosition.LastMovePlayed);
            }

            ShowOpening();

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

            UpdateFocusLabel();

            if (ShouldDrawUserArrows && CurrentPosition.Lines.Count > 0 && !isInfiniteSearch)
            {
                List<Square> list = new List<Square>();
                foreach (var item in CurrentPosition.Lines)
                {
                    if (item.Pen == null)
                    {
                        item.Pen = new Pen(new SolidBrush(item.Color), 8F);
                        System.Drawing.Drawing2D.AdjustableArrowCap aac =
                        new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                        aac.MiddleInset = 2;
                        item.Pen.CustomEndCap = aac;
                    }
                    item.Enabled = true;
                    list.AddRange(item.Squares);
                }
                RefreshSquares(list, RedrawPerspective.Arrow, null);
            }
        }
        private void RunOtherUtilities()
        {

        }
        private void timeControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTimeForm(this);
        }
        private TimeControl ShowTimeForm(IWin32Window owner)
        {
            TimeForm = new Form();
            TimeForm.MaximizeBox = false;
            TimeForm.MinimizeBox = false;
            TimeForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            TimeForm.StartPosition = FormStartPosition.CenterParent;
            TimeForm.AutoScroll = true;
            TimeForm.Text = "Set Time";
            TimeForm.Size = new System.Drawing.Size(490, 600);

            Label label1 = new Label();
            label1.AutoSize = true;
            label1.Text = "Drag the trackbar below to select main time control for the " + 
                (owner == this ? "game" : "tourney");
            label1.Location = new Point(10, 5);
            label1.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar MainTimeTB = new TrackBar();
            MainTimeTB.Maximum = 180;
            MainTimeTB.Minimum = 1;
            MainTimeTB.Name = "MainTimeTB";
            MainTimeTB.Width = 300;
            MainTimeTB.TickStyle = TickStyle.Both;
            MainTimeTB.TickFrequency = 1;
            MainTimeTB.Value = (int)timeControl.MainTime.TotalMinutes;
            MainTimeTB.Location = new Point(10, 30);
            MainTimeTB.ValueChanged += tb_ValueChanged;
            MainTimeTB.AutoSize = true;

            Label MainTimeLabel = new Label();
            MainTimeLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            MainTimeLabel.Location = new Point(MainTimeTB.Right + 5, MainTimeTB.Top + 10);
            MainTimeLabel.AutoSize = true;
            TimeSpan ts = new TimeSpan(0, MainTimeTB.Value, 0);
            MainTimeLabel.Text = (ts.Hours == 0 ? "" : (ts.Hours + (ts.Hours > 1 ? " hrs" : " hr")
                + (ts.Minutes == 0 ? "" : ", "))) + (ts.Minutes == 0 ? "" : ts.Minutes + 
                (ts.Minutes > 1 ? " mins" : " min"));
            MainTimeTB.Tag = MainTimeLabel;

            Label label2 = new Label();
            label2.AutoSize = true;
            label2.Text = "Drag the trackbar below to select main increment for the " +
                (owner == this ? "game" : "tourney");
            label2.Location = new Point(10, MainTimeTB.Bottom + 10);
            label2.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar MainIncTB = new TrackBar();
            MainIncTB.Maximum = 60;
            MainIncTB.Name = "MainIncTB";
            MainIncTB.Minimum = 0;
            MainIncTB.Width = 100;
            MainIncTB.TickStyle = TickStyle.Both;
            MainIncTB.TickFrequency = 1;
            MainIncTB.Value = timeControl.MainIncrement;
            MainIncTB.Location = new Point(MainTimeTB.Left, MainTimeTB.Bottom + 30);
            MainIncTB.ValueChanged += tb_ValueChanged;
            MainIncTB.AutoSize = true;

            Label MainIncLabel = new Label();
            MainIncLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            MainIncLabel.Location = new Point(MainIncTB.Right + 5, MainIncTB.Top + 10);
            MainIncLabel.AutoSize = true;
            ts = new TimeSpan(0, 0, MainIncTB.Value);
            MainIncLabel.Text = ts.TotalSeconds + " seconds";
            MainIncTB.Tag = MainIncLabel;

            Label label3 = new Label();
            label3.AutoSize = true;
            label3.Text = "Drag the trackbar below to select the human bonus time."
                + "\nThis is added to your main time" + (owner == this ? "in Single Player mode" : "");
            label3.Location = new Point(10, MainIncTB.Bottom + 60);
            label3.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar HumanBonusTB = new TrackBar();
            HumanBonusTB.Maximum = 120;
            HumanBonusTB.Name = "HumanBonusTB";
            HumanBonusTB.Minimum = 0;
            HumanBonusTB.Width = 200;
            HumanBonusTB.TickStyle = TickStyle.Both;
            HumanBonusTB.TickFrequency = 1;
            HumanBonusTB.Value = (int)timeControl.HumanBonusTime.TotalMinutes;
            HumanBonusTB.Location = new Point(MainTimeTB.Left, MainIncTB.Bottom + 100);
            HumanBonusTB.ValueChanged += tb_ValueChanged;
            HumanBonusTB.AutoSize = true;

            Label HumanBonusLabel = new Label();
            HumanBonusLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            HumanBonusLabel.Location = new Point(HumanBonusTB.Right + 5, HumanBonusTB.Top + 10);
            HumanBonusLabel.AutoSize = true;
            ts = new TimeSpan(0, HumanBonusTB.Value, 0);
            HumanBonusLabel.Text = (ts.Hours == 0 ? "" : (ts.Hours + (ts.Hours > 1 ? " hrs" : " hr")
                + (ts.Minutes == 0 ? "" : ", "))) + (ts.Minutes == 0 ? "" : ts.Minutes +
                (ts.Minutes > 1 ? " mins" : " min")) + (ts == TimeSpan.Zero ? " 0 seconds" : "");
            HumanBonusTB.Tag = HumanBonusLabel;

            Label label4 = new Label();
            label4.AutoSize = true;
            label4.Text = "Drag the trackbar below to select the human bonus increment."
                + "\nThis is added to your main increment" + (owner == this ? "in Single Player mode" : "");
            label4.Location = new Point(10, HumanBonusTB.Bottom + 10);
            label4.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);

            TrackBar HumanBonusIncTB = new TrackBar();
            HumanBonusIncTB.Maximum = 60;
            HumanBonusIncTB.Name = "HumanBonusIncTB";
            HumanBonusIncTB.Minimum = 0;
            HumanBonusIncTB.Width = 100;
            HumanBonusIncTB.TickStyle = TickStyle.Both;
            HumanBonusIncTB.TickFrequency = 1;
            HumanBonusIncTB.Value = timeControl.HumanBonusIncrement;
            HumanBonusIncTB.Location = new Point(MainTimeTB.Left, HumanBonusTB.Bottom + 50);
            HumanBonusIncTB.ValueChanged += tb_ValueChanged;
            HumanBonusIncTB.AutoSize = true;

            Label HumanBonusIncLabel = new Label();
            HumanBonusIncLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            HumanBonusIncLabel.Location = new Point(HumanBonusIncTB.Right + 5, HumanBonusIncTB.Top + 10);
            HumanBonusIncLabel.AutoSize = true;
            ts = new TimeSpan(0, 0, HumanBonusIncTB.Value);
            HumanBonusIncLabel.Text = ts.TotalSeconds + " seconds";
            HumanBonusIncTB.Tag = HumanBonusIncLabel;

            ComboBox cb = new ComboBox();
            cb.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Regular);
            cb.Width = 200;
            cb.Items.Add("Use for entire game");
            cb.Items.Add("Use until move. . .");
            cb.SelectedIndex = 0;
            cb.SelectedValueChanged += cb_SelectedValueChanged;
            cb.Location = new Point(130, HumanBonusIncTB.Bottom + 50);
            cb.BackColor = SystemColors.Control;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.System;

            #region TCPanel
            Panel TCPanel = new Panel();
            cb.Tag = TCPanel;
            TCPanel.Location = new Point(5, cb.Bottom + 30);
            TCPanel.Size = new Size(TimeForm.Width - 40, 150);

            Label TCLabel = new Label();
            TCLabel.AutoSize = true;
            TCLabel.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Regular);
            TCLabel.Text = "Till move: ";
            TCLabel.Location = new Point(30, 10);

            ComboBox cb2 = new ComboBox();
            cb2.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Regular);
            cb2.Width = 50;
            for (int i = 0; i < 11; i++)
                cb2.Items.Add(i * 5 + 20);
            cb2.SelectedIndex = 4;
            cb2.Location = new Point(100, TCLabel.Top - 3);
            cb2.DropDownStyle = ComboBoxStyle.DropDownList;
            cb2.Name = "cb2";
            cb2.SelectedValueChanged += cb_SelectedValueChanged;
            cb2.FlatStyle = FlatStyle.System;

            Label label5 = new Label();
            label5.AutoSize = true;
            label5.Text = "Drag the trackbar below to select the additional \ntime added" + 
                (owner == this ? " for the rest of the game" : "") + " after move " + cb2.SelectedItem;
            label5.Location = new Point(10, TCLabel.Bottom + 30);
            label5.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Italic);
            cb2.Tag = label5;

            TrackBar TCTimeTB = new TrackBar();
            TCTimeTB.Maximum = 60;
            TCTimeTB.Name = "TCTimeTB";
            TCTimeTB.Minimum = 1;
            TCTimeTB.Width = 200;
            TCTimeTB.TickStyle = TickStyle.Both;
            TCTimeTB.TickFrequency = 1;
            TCTimeTB.Value = timeControl.TCTime == TimeSpan.Zero ? 30 : 
                (int)timeControl.TCTime.TotalMinutes;
            TCTimeTB.Location = new Point(10, label5.Bottom + 20);
            TCTimeTB.ValueChanged += tb_ValueChanged;
            TCTimeTB.AutoSize = true;

            Label TCTimeLabel = new Label();
            TCTimeLabel.Font = new System.Drawing.Font("Segoe UI", 13, FontStyle.Bold);
            TCTimeLabel.Location = new Point(TCTimeTB.Right + 5, TCTimeTB.Top + 10);
            TCTimeLabel.AutoSize = true;
            ts = new TimeSpan(0, TCTimeTB.Value, 0);
            TCTimeLabel.Text = (ts.Hours == 0 ? "" : (ts.Hours + (ts.Hours > 1 ? " hrs" : " hr")
                + (ts.Minutes == 0 ? "" : ", "))) + (ts.Minutes == 0 ? "" : ts.Minutes +
                (ts.Minutes > 1 ? " mins" : " min")) + (ts == TimeSpan.Zero ? " 0 seconds" : "");
            TCTimeTB.Tag = TCTimeLabel;

            TCPanel.Controls.Add(TCLabel);
            TCPanel.Controls.Add(cb2);
            TCPanel.Controls.Add(label5);
            TCPanel.Controls.Add(TCTimeTB);
            TCPanel.Controls.Add(TCTimeLabel);
            #endregion

            Button Cancel = new Button(), OK = new Button();
            Cancel.Location = new Point(100, cb.Bottom + 30);
            OK.Location = new Point(300, Cancel.Top);
            Cancel.Text = "Cancel";
            OK.Text = "OK";
            Cancel.Font = new System.Drawing.Font("Segoe UI", 15, FontStyle.Bold);
            OK.Font = new System.Drawing.Font("Segoe UI", 15, FontStyle.Bold);
            Cancel.AutoSize = true;
            OK.AutoSize = true;
            Cancel.Click += TimeFormButton_Click;
            OK.Click += TimeFormButton_Click;

            Label buffer = new Label();
            buffer.Location = new Point(0, OK.Bottom + 15);
            buffer.Text = "  ";
            OK.Tag = buffer;
            Cancel.Tag = buffer;

            TimeForm.Tag = Tuple.Create(OK, Cancel);
            TimeForm.Controls.Add(MainTimeTB);
            TimeForm.Controls.Add(MainTimeLabel);
            TimeForm.Controls.Add(MainIncLabel);
            TimeForm.Controls.Add(MainIncTB);
            TimeForm.Controls.Add(label1);
            TimeForm.Controls.Add(label2);
            TimeForm.Controls.Add(label3);
            TimeForm.Controls.Add(label4);
            TimeForm.Controls.Add(buffer);
            TimeForm.Controls.Add(cb);
            TimeForm.Controls.Add(OK);
            TimeForm.Controls.Add(Cancel);
            TimeForm.Controls.Add(HumanBonusLabel);
            TimeForm.Controls.Add(HumanBonusTB);
            TimeForm.Controls.Add(HumanBonusIncTB);
            TimeForm.Controls.Add(HumanBonusIncLabel);
            TimeForm.ShowDialog(owner);

            if (TimeForm.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                timeControl = new TimeControl();
                timeControl.MainTime = new TimeSpan(0, MainTimeTB.Value, 0);
                timeControl.MainIncrement = MainIncTB.Value;
                timeControl.HumanBonusTime = new TimeSpan(0, HumanBonusTB.Value, 0);
                timeControl.HumanBonusIncrement = HumanBonusIncTB.Value;
                if (cb.SelectedIndex == 1)
                {
                    timeControl.TCMoveNumber = int.Parse(cb2.SelectedItem.ToString());
                    timeControl.TCTime = new TimeSpan(0, TCTimeTB.Value, 0);
                }
                else
                {
                    timeControl.TCMoveNumber = 0;
                    timeControl.TCTime = TimeSpan.Zero;
                }
                if (owner == this)       // if settings are right. Which settings?? Can't remember!
                {
                    WhiteTime.TimeLeft = timeControl.MainTime + (WhiteTime.Player is User &&
                        ModeOfPlay == PlayMode.SinglePlayer ? timeControl.HumanBonusTime : TimeSpan.Zero);
                    WhiteTime.Increment = timeControl.MainIncrement + (WhiteTime.Player is User &&
                        ModeOfPlay == PlayMode.SinglePlayer ? timeControl.HumanBonusIncrement : 0);
                    BlackTime.TimeLeft = timeControl.MainTime + (BlackTime.Player is User &&
                        ModeOfPlay == PlayMode.SinglePlayer ? timeControl.HumanBonusTime : TimeSpan.Zero);
                    BlackTime.Increment = timeControl.MainIncrement + (BlackTime.Player is User &&
                        ModeOfPlay == PlayMode.SinglePlayer ? timeControl.HumanBonusIncrement : 0);

                    WhiteClockLabel.Text = WhiteTime.ToString(true);
                    BlackClockLabel.Text = BlackTime.ToString(true);

                    return timeControl;
                }
                else
                {
                    return timeControl;
                }
            }
            else
                return null;
        }
        void TimeFormButton_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "OK")
            {
                TimeForm.DialogResult = System.Windows.Forms.DialogResult.OK;
                TimeForm.Close();
            }
            else if ((sender as Button).Text == "Cancel")
            {
                TimeForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                TimeForm.Close();
            }
        }
        void cb_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.Name == "cb2")
            {
                (cb.Tag as Label).Text = "Drag the trackbar below to select the additional \ntime added for the rest of the game"
                + " after move " + cb.SelectedItem;
                return;
            }
            if ((string)cb.SelectedItem == "Use until move. . .")
            {
                TimeForm.Controls.Add(cb.Tag as Panel);
                (TimeForm.Tag as Tuple<Button, Button>).Item1.Location +=
                    new Size(0, (cb.Tag as Panel).Height + 10);
                (TimeForm.Tag as Tuple<Button, Button>).Item2.Location +=
                    new Size(0, (cb.Tag as Panel).Height + 10);

                ((TimeForm.Tag as Tuple<Button, Button>).Item1.Tag as Label).Location +=
                    new Size(0, (cb.Tag as Panel).Height + 10);
            }
            else
            {
                TimeForm.Controls.Remove(cb.Tag as Panel);
                (TimeForm.Tag as Tuple<Button, Button>).Item1.Location -=
                    new Size(0, (cb.Tag as Panel).Height + 10);
                (TimeForm.Tag as Tuple<Button, Button>).Item2.Location -=
                    new Size(0, (cb.Tag as Panel).Height + 10);

                ((TimeForm.Tag as Tuple<Button, Button>).Item1.Tag as Label).Location -=
                    new Size(0, (cb.Tag as Panel).Height + 10);
            }
        }
        void tb_ValueChanged(object sender, EventArgs e)
        {
            TrackBar tb = (sender as TrackBar);

            if (tb.Name == "MainTimeTB" || tb.Name == "HumanBonusTB" || tb.Name == "TCTimeTB")
            {
                TimeSpan ts = new TimeSpan(0, tb.Value, 0);
                (tb.Tag as Label).Text = (ts.Hours == 0 ? "" : (ts.Hours + (ts.Hours > 1 ? " hrs" : " hr")
                    + (ts.Minutes == 0 ? "" : ", "))) + (ts.Minutes == 0 ? "" : ts.Minutes +
                    (ts.Minutes > 1 ? " mins" : " min")) + (ts == TimeSpan.Zero ? " 0 seconds" : "");
            }
            else if (tb.Name == "MainIncTB" || tb.Name == "HumanBonusIncTB")
            {
                TimeSpan ts = new TimeSpan(0, 0, tb.Value);
                (tb.Tag as Label).Text = ts.TotalSeconds + " seconds";
            }
        }
        private void PauseClocks()
        {
            if (WhiteTime.isTicking)
            {
                WhiteTime.TimeLeft -= 
                        (DateTime.Now - WhiteTime.Differential - new TimeSpan(0, 0, 0, 0, 15));
                if (WhiteTime.TimeLeft >= TimeSpan.Zero)
                    WhiteClockLabel.Text = WhiteTime.ToString(true);
            }
            else if (BlackTime.isTicking)
            {
                BlackTime.TimeLeft -= 
                        (DateTime.Now - BlackTime.Differential - new TimeSpan(0, 0, 0, 0, 15));
                if (BlackTime.TimeLeft >= TimeSpan.Zero)
                    BlackClockLabel.Text = BlackTime.ToString(true);
            }
            isTimePaused = true;
        }
        private void RestartClocks()
        {
            if (WhiteTime.isTicking)
            {
                WhiteTime.Differential = DateTime.Now;
            }
            else if (BlackTime.isTicking)
            {
                BlackTime.Differential = DateTime.Now;
            }
            isTimePaused = false;
        }
        private void changeTimeControlMenuItem_Click(object sender, EventArgs e)
        {
            ShowTimeForm(this);
        }
        private void pauseClocksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            IPlayer player = CurrentPlayer.Time.isTicking ? CurrentPlayer : null;
            if (player == null)
                return;
            if (tsmi.Text == "Pause Clocks")
            {
                if (player is User)
                {
                    PauseClocks();
                    tsmi.Text = "Restart Clocks";
                }
                else if (player is Engine)
                {
                    PausePending = true;
                    tsmi.Text = "Undo pending Pause";
                    MessageBox.Show("The clocks will be paused after " + player.Name
                        + " completes its move. To change this, Right-click and select \"Undo pending Pause\"");
                }
            }
            else if (tsmi.Text == "Restart Clocks")
            {
                RestartClocks();
                tsmi.Text = "Pause Clocks";
            }
            else if (tsmi.Text == "Undo pending Pause")
            {
                PausePending = false;
                tsmi.Text = "Pause Clocks";
            }
        }
        private void ClockLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                timeContextMenuStrip.Tag = (sender as Control).Tag;
        }
        private void timeContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timeContextMenuStrip.Tag == null || !(timeContextMenuStrip.Tag is IPlayer))
            {
                foreach (var item in timeContextMenuStrip.Items)
                    (item as ToolStripItem).Enabled = false;

                if (WhiteTime.isTicking || BlackTime.isTicking)
                    pauseClocksToolStripMenuItem.Enabled = true;
                else
                    pauseClocksToolStripMenuItem.Enabled = false;

                changeTimeControlMenuItem.Enabled = true;
                return;
            }

            IPlayer player = timeContextMenuStrip.Tag as IPlayer;

            if (WhiteTime.isTicking || BlackTime.isTicking && 
                (ModeOfPlay == PlayMode.SinglePlayer || ModeOfPlay == PlayMode.TwoPlayer))
                pauseClocksToolStripMenuItem.Enabled = true;
            else
                pauseClocksToolStripMenuItem.Enabled = false;

            if (ModeOfPlay == PlayMode.EditPosition)
            {
                addAMinuteToolStripMenuItem.Enabled = false;
                subtractAMinuteToolStripMenuItem.Enabled = false;
            }
            else
            {
                addAMinuteToolStripMenuItem.Enabled = true;
                if (player.Time.TimeLeft > new TimeSpan(0, 1, 30))
                    subtractAMinuteToolStripMenuItem.Enabled = true;
                else
                    subtractAMinuteToolStripMenuItem.Enabled = false;
            }
        }
        private void addAMinuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPlayer player = timeContextMenuStrip.Tag as IPlayer;
            if (player == null)
                return;
            player.Time.TimeLeft += new TimeSpan(0, 1, 0);
            player.Time.ClockLabel.Text = player.Time.ToString();
        }
        private void subtractAMinuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPlayer player = timeContextMenuStrip.Tag as IPlayer;
            if (player == null)
                return;
            player.Time.TimeLeft -= new TimeSpan(0, 1, 0);
            player.Time.ClockLabel.Text = player.Time.ToString();
        }
        private void ClockPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                timeContextMenuStrip.Tag = null;
        }
        private void ClockLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                ShowTimeForm(this);
        }
        private void modeToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var item in modeToolStripMenuItem.DropDownItems)
                if (item is ToolStripMenuItem)
                    (item as ToolStripMenuItem).Checked = false;
            switch (ModeOfPlay)
            {
                case PlayMode.SinglePlayer:
                    singlePlayerToolStripMenuItem.Checked = true;
                    break;
                case PlayMode.TwoPlayer:
                    twoPlayerToolStripMenuItem.Checked = true;
                    break;
                case PlayMode.EngineVsEngine:
                    engineVsEngineToolStripMenuItem.Checked = true;
                    break;
                case PlayMode.EditPosition:
                    enterMovesToolStripMenuItem.Checked = true;
                    break;
                default:
                    break;
            }
        }
        private void copyPositionToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(GetFENString());
            }
            catch (Exception)
            {

            }
        }
        private void pastePostionFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LoadFEN(Clipboard.GetText());
            }
            catch (Exception)
            {

            }
        }
        private void copyGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGame();
        }
        private void pasteGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteGameFromClipBoard();
        }
        private void flipBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FlipBoard();
        }
        [Serializable]
        public class TimeControl
        {
            public TimeControl()
            {
                TillEndOfGame = true;
                MainTime = TimeSpan.Zero;
                TCTime = TimeSpan.Zero;
                HumanBonusTime = TimeSpan.Zero;
            }
            public TimeSpan MainTime { get; set; }
            public int MainIncrement { get; set; }
            public bool TillEndOfGame { get; set; }
            public TimeSpan TCTime { get; set; }
            public int TCMoveNumber { get; set; }
            public TimeSpan HumanBonusTime { get; set; }
            public int HumanBonusIncrement { get; set; }
            public string GetDescription()
            {
                return MainTime.TotalMinutes.ToString() + " minutes" + ((TCMoveNumber != 0 && TCTime != TimeSpan.Zero) ? 
                    " till move " + TCMoveNumber + " and then, " + TCTime.TotalMinutes.ToString() + 
                    " additional minutes for the rest of the game" 
                    : " for the entire game")
                    + (MainIncrement > 0 ? ", with " + MainIncrement + " seconds added after each move" : "")
                    + (HumanBonusTime != TimeSpan.Zero ? ". (Human bonus: " + HumanBonusTime.TotalMinutes + " extra minutes" 
                    + (HumanBonusIncrement != 0 ? ", with extra " + HumanBonusIncrement + " seconds increment)" : ")")
                    : "");
            }
            public override string ToString()
            {
                return (TCMoveNumber != 0 ? TCMoveNumber + "/" : "") + MainTime.TotalMinutes.ToString() + "'" +
                    (MainIncrement != 0 ? " + " + MainIncrement + "\"" : "") +
                    (TCTime != TimeSpan.Zero ? ", " + TCTime.TotalMinutes + "'" : "") +
                    (HumanBonusTime != TimeSpan.Zero ? " (Human Bonus: " + HumanBonusTime.TotalMinutes + "'" +
                    (HumanBonusIncrement != 0 ? " + " + HumanBonusIncrement + "\")" : ")") : "");
            }
        }
        [Serializable]
        public class State
        {
            public State()
            {
                this.TimeControl = new TimeControl();
                this.TimeControl.MainTime = new TimeSpan(0, 3, 0);
                this.TimeControl.MainIncrement = 2;

                this.WhiteTime = new Clock();
                this.BlackTime = new Clock();
                this.WhiteTime.TimeLeft = TimeControl.MainTime;
                this.WhiteTime.Increment = TimeControl.MainIncrement;
                this.BlackTime.TimeLeft = TimeControl.MainTime;
                this.BlackTime.Increment = TimeControl.MainIncrement;

                this.UserList = new List<User>();
                this.CurrentUser = new User(Environment.UserName);
                this.UserList.Add(this.CurrentUser);

                this.ModeOfPlay = PlayMode.SinglePlayer;     // What happens if user makes a move and engine 
                                                             // neva load e.g, Spike. New Precautions :-)
                this.ShouldClockTick = true;
                this.FENForStartPosition = "";
            }
            public Engine FirstEngine { get; set; }
            public Engine SecondEngine { get; set; }
            public Square[,] TwoDSquares { get; set; }
            public String FENForStartPosition { get; set; }
            public Clock WhiteTime { get; set; }
            public Clock BlackTime { get; set; }
            public Game Game { get; set; }
            public List<User> UserList { get; set; }
            public Position StartingPosition { get; set; }
            public User CurrentUser { get; set; }
            public List<Square> Squares { get; set; }
            public bool isRatedGameInProgress { get; set; }
            public String CurrentPgnDBPath { get; set; }
            public bool isBoardFlipped { get; set; }
            public int CurrentPgnIndex { get; set; }
            public bool ShouldClockTick { get; set; }
            public bool isEngineMatchInProgress { get; set; }
            public PlayMode ModeOfPlay { get; set; }
            public IPlayer CurrentPlayer { get; set; }
            public TimeControl TimeControl { get; set; }
        }
        [Serializable]
        public class Settings
        {
            public Settings()
            {
                appearance = new Appearance();
                appearance.ColorTheme = PresetTheme.Theme.Linen;
                appearance.BoardCordinatesSides = "All sides";
                appearance.ChessFont = ChessFont.Merida;
                appearance.KICHLType = "Glow";
                appearance.LastMoveHLType = "Shade";
                appearance.AnimationType = "Fast";
                appearance.SaveUserArrows = "Save with position";
                appearance.ShouldShowBMA = true;
                appearance.ShouldShowKICHL = true;
                appearance.ShouldShowLastMoveHL = true;
                appearance.ShouldShowLegalMovesHL = true;
                appearance.ShouldShowUserArrows = true;
                appearance.UserArrowSquareHLType = "Shade";

                engine = new Engine();
                engine.Ponder = true;
                engine.ThreadCount = "Engine Default";
                engine.OpeningBookUse = "Always use Default Book";
                engine.HashSizeHandling = "Always ask";
                engine.HashOptionList = new List<Tuple<Form1.Engine, string>>();

                behaviour = new Behaviour();
                behaviour.AllowTimeChangesDuringGame = false;
                behaviour.NewMoveHandling = "Always ask";
                behaviour.PromotionType = "Always ask";
                behaviour.RatedGamesStrictnessLevel = "No takebacks or arbitrary quitting";
                behaviour.ResignAndOfferDraw = true;
                behaviour.TimeForfeit = true;

                miscellaneous = new Miscellaneous();
                miscellaneous.AutoSaveForNonRatedGames = "After each game";
                miscellaneous.AutoSaveForRatedGames = "After each move";
                miscellaneous.MoveSoundTheme = "Classic";
                miscellaneous.ShouldPlaySounds = true;
            }

            public Appearance appearance { get; set; }
            public Engine engine { get; set; }
            public Behaviour behaviour { get; set; }
            public Miscellaneous miscellaneous { get; set; }

            [Serializable]
            public class Appearance
            {
                public PresetTheme.Theme ColorTheme { get; set; }
                public ChessFont ChessFont { get; set; }
                public String AnimationType { get; set; }
                public bool ShouldShowLastMoveHL { get; set; }
                public String LastMoveHLType { get; set; }
                public bool ShouldShowKICHL { get; set; }
                public String KICHLType { get; set; }
                public bool ShouldShowLegalMovesHL { get; set; }
                public bool ShouldShowUserArrows { get; set; }
                public String SaveUserArrows { get; set; }
                public String UserArrowSquareHLType { get; set; }
                public bool ShouldShowBMA { get; set; }
                public String BoardCordinatesSides { get; set; }
            }
            [Serializable]
            public class Engine
            {
                public bool Ponder { get; set; }
                public String HashSizeHandling { get; set; }
                public String OpeningBookUse { get; set; }
                public String ThreadCount { get; set; }
                public List<Tuple<Form1.Engine, String>> HashOptionList { get; set; }
            }
            [Serializable]
            public class Behaviour
            {
                public bool TimeForfeit { get; set; }
                public bool ResignAndOfferDraw { get; set; }
                public String RatedGamesStrictnessLevel { get; set; }
                public bool AllowTimeChangesDuringGame { get; set; }
                public String PromotionType { get; set; }
                public String NewMoveHandling { get; set; }
            }
            [Serializable]
            public class Miscellaneous
            {
                public String AutoSaveForRatedGames { get; set; }
                public String AutoSaveForNonRatedGames { get; set; }
                public bool ShouldPlaySounds { get; set; }
                public String MoveSoundTheme { get; set; }
            }
        }
    }
}
