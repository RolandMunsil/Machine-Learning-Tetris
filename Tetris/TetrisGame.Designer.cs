namespace Tetris
{
    partial class TetrisGame
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.learnButton = new System.Windows.Forms.Button();
            this.scoreLabel = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGameButton = new System.Windows.Forms.Button();
            this.rowsClearedLabel = new System.Windows.Forms.Label();
            this.rowsCleared = new System.Windows.Forms.Label();
            this.gameWindow = new System.Windows.Forms.Panel();
            this.rotateOutputLabel = new System.Windows.Forms.Label();
            this.downOutputLabel = new System.Windows.Forms.Label();
            this.rightOutputLabel = new System.Windows.Forms.Label();
            this.leftOutputLabel = new System.Windows.Forms.Label();
            this.upcomingBlocks = new System.Windows.Forms.Panel();
            this.fitnessLabel = new System.Windows.Forms.Label();
            this.organismLabel = new System.Windows.Forms.Label();
            this.speciesLabel = new System.Windows.Forms.Label();
            this.genLabel = new System.Windows.Forms.Label();
            this.score = new System.Windows.Forms.Label();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tableLayout.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.gameWindow.SuspendLayout();
            this.upcomingBlocks.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayout
            // 
            this.tableLayout.ColumnCount = 3;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 84.28835F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.71165F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 184F));
            this.tableLayout.Controls.Add(this.learnButton, 2, 2);
            this.tableLayout.Controls.Add(this.scoreLabel, 0, 5);
            this.tableLayout.Controls.Add(this.menuStrip, 0, 0);
            this.tableLayout.Controls.Add(this.newGameButton, 1, 2);
            this.tableLayout.Controls.Add(this.rowsClearedLabel, 1, 4);
            this.tableLayout.Controls.Add(this.rowsCleared, 2, 4);
            this.tableLayout.Controls.Add(this.gameWindow, 0, 1);
            this.tableLayout.Controls.Add(this.upcomingBlocks, 1, 1);
            this.tableLayout.Controls.Add(this.score, 2, 5);
            this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayout.Location = new System.Drawing.Point(0, 0);
            this.tableLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.RowCount = 6;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.657402F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.15385F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.179487F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 81F));
            this.tableLayout.Size = new System.Drawing.Size(1484, 806);
            this.tableLayout.TabIndex = 0;
            // 
            // learnButton
            // 
            this.learnButton.AutoSize = true;
            this.tableLayout.SetColumnSpan(this.learnButton, 2);
            this.learnButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.learnButton.Location = new System.Drawing.Point(1181, 630);
            this.learnButton.Name = "learnButton";
            this.learnButton.Size = new System.Drawing.Size(300, 39);
            this.learnButton.TabIndex = 10;
            this.learnButton.Text = "Learn";
            this.learnButton.UseVisualStyleBackColor = true;
            this.learnButton.Click += new System.EventHandler(this.learnButton_Click);
            // 
            // scoreLabel
            // 
            this.scoreLabel.AutoSize = true;
            this.scoreLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scoreLabel.Location = new System.Drawing.Point(1099, 724);
            this.scoreLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.scoreLabel.Name = "scoreLabel";
            this.scoreLabel.Size = new System.Drawing.Size(89, 29);
            this.scoreLabel.TabIndex = 8;
            this.scoreLabel.Text = "Score:";
            // 
            // menuStrip
            // 
            this.tableLayout.SetColumnSpan(this.menuStrip, 4);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip.Size = new System.Drawing.Size(1484, 35);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newGameToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(70, 29);
            this.gameToolStripMenuItem.Text = "Game";
            // 
            // newGameToolStripMenuItem
            // 
            this.newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            this.newGameToolStripMenuItem.Size = new System.Drawing.Size(183, 30);
            this.newGameToolStripMenuItem.Text = "New Game";
            // 
            // newGameButton
            // 
            this.tableLayout.SetColumnSpan(this.newGameButton, 2);
            this.newGameButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.newGameButton.Location = new System.Drawing.Point(1099, 587);
            this.newGameButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.newGameButton.Name = "newGameButton";
            this.newGameButton.Size = new System.Drawing.Size(381, 35);
            this.newGameButton.TabIndex = 3;
            this.newGameButton.Text = "New Game";
            this.newGameButton.UseVisualStyleBackColor = true;
            this.newGameButton.Click += new System.EventHandler(this.newGameButton_Click);
            // 
            // rowsClearedLabel
            // 
            this.rowsClearedLabel.AutoSize = true;
            this.rowsClearedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rowsClearedLabel.Location = new System.Drawing.Point(1099, 672);
            this.rowsClearedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.rowsClearedLabel.Name = "rowsClearedLabel";
            this.rowsClearedLabel.Size = new System.Drawing.Size(85, 29);
            this.rowsClearedLabel.TabIndex = 4;
            this.rowsClearedLabel.Text = "Rows:";
            // 
            // rowsCleared
            // 
            this.rowsCleared.AutoSize = true;
            this.rowsCleared.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rowsCleared.Location = new System.Drawing.Point(1303, 672);
            this.rowsCleared.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.rowsCleared.Name = "rowsCleared";
            this.rowsCleared.Size = new System.Drawing.Size(27, 29);
            this.rowsCleared.TabIndex = 5;
            this.rowsCleared.Text = "0";
            // 
            // gameWindow
            // 
            this.gameWindow.Controls.Add(this.rotateOutputLabel);
            this.gameWindow.Controls.Add(this.downOutputLabel);
            this.gameWindow.Controls.Add(this.rightOutputLabel);
            this.gameWindow.Controls.Add(this.leftOutputLabel);
            this.gameWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameWindow.Location = new System.Drawing.Point(4, 46);
            this.gameWindow.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gameWindow.Name = "gameWindow";
            this.tableLayout.SetRowSpan(this.gameWindow, 5);
            this.gameWindow.Size = new System.Drawing.Size(1087, 755);
            this.gameWindow.TabIndex = 6;
            // 
            // rotateOutputLabel
            // 
            this.rotateOutputLabel.BackColor = System.Drawing.SystemColors.Info;
            this.rotateOutputLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rotateOutputLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.rotateOutputLabel.Font = new System.Drawing.Font("Arial Black", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rotateOutputLabel.Location = new System.Drawing.Point(995, 502);
            this.rotateOutputLabel.Name = "rotateOutputLabel";
            this.rotateOutputLabel.Size = new System.Drawing.Size(50, 50);
            this.rotateOutputLabel.TabIndex = 3;
            this.rotateOutputLabel.Text = "↺";
            this.rotateOutputLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // downOutputLabel
            // 
            this.downOutputLabel.BackColor = System.Drawing.SystemColors.Info;
            this.downOutputLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.downOutputLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.downOutputLabel.Font = new System.Drawing.Font("Arial Black", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.downOutputLabel.Location = new System.Drawing.Point(995, 342);
            this.downOutputLabel.Name = "downOutputLabel";
            this.downOutputLabel.Size = new System.Drawing.Size(50, 50);
            this.downOutputLabel.TabIndex = 2;
            this.downOutputLabel.Text = "↓";
            this.downOutputLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rightOutputLabel
            // 
            this.rightOutputLabel.BackColor = System.Drawing.SystemColors.Info;
            this.rightOutputLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rightOutputLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.rightOutputLabel.Font = new System.Drawing.Font("Arial Black", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rightOutputLabel.Location = new System.Drawing.Point(995, 192);
            this.rightOutputLabel.Name = "rightOutputLabel";
            this.rightOutputLabel.Size = new System.Drawing.Size(50, 50);
            this.rightOutputLabel.TabIndex = 1;
            this.rightOutputLabel.Text = "→";
            this.rightOutputLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // leftOutputLabel
            // 
            this.leftOutputLabel.BackColor = System.Drawing.SystemColors.Info;
            this.leftOutputLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.leftOutputLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.leftOutputLabel.Font = new System.Drawing.Font("Arial Black", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.leftOutputLabel.Location = new System.Drawing.Point(995, 44);
            this.leftOutputLabel.Name = "leftOutputLabel";
            this.leftOutputLabel.Size = new System.Drawing.Size(50, 50);
            this.leftOutputLabel.TabIndex = 0;
            this.leftOutputLabel.Text = "←";
            this.leftOutputLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // upcomingBlocks
            // 
            this.tableLayout.SetColumnSpan(this.upcomingBlocks, 2);
            this.upcomingBlocks.Controls.Add(this.textBox1);
            this.upcomingBlocks.Controls.Add(this.fitnessLabel);
            this.upcomingBlocks.Controls.Add(this.organismLabel);
            this.upcomingBlocks.Controls.Add(this.speciesLabel);
            this.upcomingBlocks.Controls.Add(this.genLabel);
            this.upcomingBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.upcomingBlocks.Location = new System.Drawing.Point(1099, 46);
            this.upcomingBlocks.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.upcomingBlocks.Name = "upcomingBlocks";
            this.upcomingBlocks.Size = new System.Drawing.Size(381, 531);
            this.upcomingBlocks.TabIndex = 7;
            // 
            // fitnessLabel
            // 
            this.fitnessLabel.AutoSize = true;
            this.fitnessLabel.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fitnessLabel.Location = new System.Drawing.Point(13, 154);
            this.fitnessLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fitnessLabel.Name = "fitnessLabel";
            this.fitnessLabel.Size = new System.Drawing.Size(166, 25);
            this.fitnessLabel.TabIndex = 17;
            this.fitnessLabel.Text = "Original Fitness:";
            // 
            // organismLabel
            // 
            this.organismLabel.AutoSize = true;
            this.organismLabel.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.organismLabel.Location = new System.Drawing.Point(13, 96);
            this.organismLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.organismLabel.Name = "organismLabel";
            this.organismLabel.Size = new System.Drawing.Size(105, 25);
            this.organismLabel.TabIndex = 16;
            this.organismLabel.Text = "Organism";
            // 
            // speciesLabel
            // 
            this.speciesLabel.AutoSize = true;
            this.speciesLabel.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.speciesLabel.Location = new System.Drawing.Point(13, 55);
            this.speciesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.speciesLabel.Name = "speciesLabel";
            this.speciesLabel.Size = new System.Drawing.Size(90, 25);
            this.speciesLabel.TabIndex = 15;
            this.speciesLabel.Text = "Species";
            // 
            // genLabel
            // 
            this.genLabel.AutoSize = true;
            this.genLabel.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.genLabel.Location = new System.Drawing.Point(13, 15);
            this.genLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.genLabel.Name = "genLabel";
            this.genLabel.Size = new System.Drawing.Size(118, 25);
            this.genLabel.TabIndex = 14;
            this.genLabel.Text = "Generation";
            // 
            // score
            // 
            this.score.AutoSize = true;
            this.score.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.score.Location = new System.Drawing.Point(1303, 724);
            this.score.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.score.Name = "score";
            this.score.Size = new System.Drawing.Size(27, 29);
            this.score.TabIndex = 9;
            this.score.Text = "0";
            // 
            // tickTimer
            // 
            this.tickTimer.Interval = 500;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(18, 216);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(343, 297);
            this.textBox1.TabIndex = 18;
            // 
            // TetrisGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1484, 806);
            this.Controls.Add(this.tableLayout);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TetrisGame";
            this.Text = "Tetris";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TetrisGame_KeyPress);
            this.tableLayout.ResumeLayout(false);
            this.tableLayout.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.gameWindow.ResumeLayout(false);
            this.upcomingBlocks.ResumeLayout(false);
            this.upcomingBlocks.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Timer tickTimer;
        public System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripMenuItem;
        private System.Windows.Forms.Panel gameWindow;
        private System.Windows.Forms.Label scoreLabel;
        private System.Windows.Forms.Button newGameButton;
        private System.Windows.Forms.Label rowsClearedLabel;
        private System.Windows.Forms.Label rowsCleared;
        private System.Windows.Forms.Panel upcomingBlocks;
        private System.Windows.Forms.Label score;
        private System.Windows.Forms.Button learnButton;
        private System.Windows.Forms.Label fitnessLabel;
        private System.Windows.Forms.Label organismLabel;
        private System.Windows.Forms.Label speciesLabel;
        private System.Windows.Forms.Label genLabel;
        private System.Windows.Forms.Label rotateOutputLabel;
        private System.Windows.Forms.Label downOutputLabel;
        private System.Windows.Forms.Label rightOutputLabel;
        private System.Windows.Forms.Label leftOutputLabel;
        private System.Windows.Forms.TextBox textBox1;
    }
}

