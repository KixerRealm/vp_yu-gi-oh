﻿using Middleware.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yu_gi_oh
{
    public partial class Deckbuilder : Form
    {
        public int currentPage { get; set; } = 0;
        public int maxPage { get; set; } = int.MaxValue;

        public BindingList<CardDto> cards = new();
        public BindingList<CardDto> deckCards = new();

        public readonly Dictionary<string, string> dgvNaming = new() { { "img", "" }, { "name", "Name" }, { "type", "Type" }, { "atk", "ATK" }, { "def", "DEF" } };

        public Deck deck { get; set; } = new Deck();
        public Deckbuilder()
        {
            InitializeComponent();
            init();
        }

        public void ConstructDGV(DataGridView dgv, BindingList<CardDto> source)
        {
            // Base configurations
            dgv.AllowUserToAddRows = false;

            dgv.AutoGenerateColumns = true;
            dgv.DataSource = source;
            dgv.AutoGenerateColumns = false;

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.RowTemplate.Height = 100;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Disable useless columns
            dgv.Columns["id"].Visible = false;
            dgv.Columns["cardId"].Visible = false;
            dgv.Columns["description"].Visible = false;
            dgv.Columns["subType"].Visible = false;

            int idx = 0;
            foreach (KeyValuePair<string, string> item in dgvNaming)
            {
                DataGridViewColumn col = dgv.Columns[item.Key];
                col.HeaderText = item.Value;
                col.DisplayIndex = idx++;
                col.ReadOnly = true;

                // Set images as Stretch
                if (item.Key == "img")
                {
                    (col as DataGridViewImageColumn).ImageLayout = DataGridViewImageCellLayout.Stretch;
                    col.Width = 60;
                }
            }
        }

        public void init()
        {
            ConstructDGV(dgv1, cards);
            ConstructDGV(dgvDeck, deckCards);
            btnNewDeck.Enabled = false;
            loadingPB.Visible = true;
            LoadDataTable(Middleware.Models.Meta.Direction.FORWARDS);
            btnRefresh.BackgroundImageLayout = ImageLayout.Stretch;

        }


        private void LoadDataTable(Middleware.Models.Meta.Direction direction)
        {
            currentPage += (int)direction;

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            Middleware.Controllers.CardController.GetAllCardDtosShortAsync(new CardDto(), currentPage).ContinueWith(t =>
            {
                Invoke((MethodInvoker)(() => CleanCards()));

                Middleware.Models.Meta.PageResponse<CardDto> page = t.Result;
                maxPage = page.totalPages;

                foreach (CardDto card in page.content)
                {
                    card.img = Middleware.Controllers.YGOController.GetImage(card.cardId);
                    Invoke((MethodInvoker)(() => cards.Add(card)));
                }

                Invoke((MethodInvoker)(() => loadingPB.Visible = false));
            });
        }

        private void CleanCards()
        {
            foreach (CardDto card in cards)
            {
                if(card.img != null) card.img.Dispose();
            }
            cards.Clear();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainMenu form = new MainMenu();
            form.ShowDialog();
            this.Close();
        }

        private void btnAddtoDeck_Click(object sender, EventArgs e)
        {
                if(dgv1.SelectedRows.Count >0)
                    btnNewDeck.Enabled = true;         
            if (dgv1.RowCount >=0)
            {
                if (isMorethan3(cards[dgv1.CurrentCell.RowIndex]))
                {
                    MessageBox.Show("You can't add more than 3 instances of that card!", "ERROR");
                   return;
                }
                     if (maxCards())
                    {
                         MessageBox.Show("The maximum number of cards for a deck is 30 !", "ERROR");
                         return;
                   }
                   
                foreach (DataGridViewRow row in dgv1.SelectedRows)
                {
                    DataGridViewRow dgvR = row;
                    deckCards.Add(new CardDto(dgvR.Cells["id"].Value.ToString(), dgvR.Cells["cardId"].Value.ToString(), dgvR.Cells["type"].Value.ToString(),dgvR.Cells["name"].Value.ToString(),dgvR.Cells["description"].Value.ToString(),dgvR.Cells["subType"].Value.ToString(), int.Parse(dgvR.Cells["atk"].Value.ToString()),int.Parse( dgvR.Cells["def"].Value.ToString()), Middleware.Controllers.YGOController.GetImage(dgvR.Cells["cardId"].Value.ToString())));                 
                }
            } 
        }

        private bool isMorethan3(CardDto c)
        {
            int ctr = 0;
            if (dgvDeck.Rows.Count > 0)
            {
               foreach (CardDto card in deckCards)
                {
                    if (card.name == c.name)
                   {
                       ctr++;
                   }
               }
            }
            if (ctr >= 3) return true;
            else return false;
        }
        private bool maxCards()
        {
            return dgvDeck.Rows.Count > 30;        
        }

        private void btnRemoveFromDeck_Click(object sender, EventArgs e)
        {
         
          foreach (DataGridViewRow row in this.dgvDeck.SelectedRows)
                {
                    DataGridViewRow dgvR = row;
                    CardDto card = new CardDto(dgvR.Cells["id"].Value.ToString(), dgvR.Cells["cardId"].Value.ToString(), dgvR.Cells["type"].Value.ToString(), dgvR.Cells["name"].Value.ToString(), dgvR.Cells["description"].Value.ToString(), dgvR.Cells["subType"].Value.ToString(), int.Parse(dgvR.Cells["atk"].Value.ToString()), int.Parse(dgvR.Cells["def"].Value.ToString()), Middleware.Controllers.YGOController.GetImage(dgvR.Cells["cardId"].Value.ToString()));
                        deckCards.Remove(card);
                        dgvDeck.Rows.RemoveAt(dgvR.Index);              
                }
                
         }
        

        private void btnSaveDeck_Click(object sender, EventArgs e)
        {
            if (dgvDeck.Rows.Count>0)
            {


                foreach (CardDto card in deckCards)
                {
                    deck.cards.Add(card);
                }

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = ".ygo";
                sfd.Filter = "ygo files (*.ygo)|*.ygo|All files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    saveAsync(sfd.FileName);
                }
              

            }
        }

        private async void saveAsync(string s)
        {
            using (FileStream fs = new FileStream(s, FileMode.Create))
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(fs, deck);
            }
        }

        private void btnNewDeck_Click(object sender, EventArgs e)
        {
            if (deckCards.Count > 0)
            {
                btnNewDeck.Enabled = true;
                if (MessageBox.Show("Are You sure you want to remove all cards from your deck ?", "Are you sure ?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    btnNewDeck.Enabled = false;
                    dgvDeck.Rows.Clear();
                }
                else
                {
                    return;
                }

            }
            
           
        }

        private void btnOpenDeck_Click(object sender, EventArgs e)
        {
            btnNewDeck.Enabled = true;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.DefaultExt = ".ygo";
            ofd.Filter = "ygo files (*.ygo)|*.ygo|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                dgvDeck.Rows.Clear();
                openAsync(ofd.FileName);
            }
        }
        private async void openAsync(string s)
        {
            using (FileStream fs = new FileStream(s, FileMode.Open))
            {

               Deck deck = await System.Text.Json.JsonSerializer.DeserializeAsync<Deck>(fs);
                foreach (CardDto card in deck.cards)
                {
                    card.img = Middleware.Controllers.YGOController.GetImage(card.cardId);
                    deckCards.Add(card);
                }

            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Deckbuilder_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            loadingPB.Visible = true;
            LoadDataTable(Middleware.Models.Meta.Direction.FORWARDS);
            button1.Enabled = true;
            if (currentPage >= maxPage) button2.Enabled = false;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            loadingPB.Visible = true;
            LoadDataTable(Middleware.Models.Meta.Direction.BACKWARDS);
            button2.Enabled = true;
            if (currentPage <= 1) button1.Enabled = false;
        }

        private void dgv1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex != -1)
            {
                DataGridViewRow dgvR = dgv1.Rows[e.RowIndex];
                rtbDescription.Text =  dgvR.Cells["description"].Value.ToString();
                pbCardImage.BackgroundImage = Middleware.Controllers.YGOController.GetImage(dgvR.Cells["cardId"].Value.ToString());      
                
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            foreach (CardDto card in cards)
            {
                card.img = Middleware.Controllers.YGOController.GetImage(card.cardId);
            }
            foreach(CardDto card in deckCards)
            {
                card.img = Middleware.Controllers.YGOController.GetImage(card.cardId);
            }
           
            dgvDeck.Refresh();
            dgv1.Refresh();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (tbCardName.Text != "" || cbCardType.Text != "" || nudATK.Value != 0 || nudDEF.Value != 0)
            {
                currentPage = 1;
                CardDto card = new CardDto();
                card.name = tbCardName.Text;
                card.atk = (int)nudATK.Value;
                if (cbCardType.Text == "")
                {
                    card.type = null;
                }
                else {
                    
                   card.type = cbCardType.Text;
                 }
                card.def = (int)nudDEF.Value;
                
                Middleware.Controllers.CardController.GetAllCardDtosShortAsync(card, currentPage).ContinueWith(t =>
                {
                    Invoke((MethodInvoker)(() => CleanCards()));

                    Middleware.Models.Meta.PageResponse<CardDto> page = t.Result;
                    maxPage = page.totalPages;

                    foreach (CardDto card in page.content)
                    {
                        card.img = Middleware.Controllers.YGOController.GetImage(card.cardId);
                        Invoke((MethodInvoker)(() => cards.Add(card)));
                    }

                    Invoke((MethodInvoker)(() => loadingPB.Visible = false));
                });

            }

        }
    }
}