using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro_AlphaBeta
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        const int n = 15;
        const int m = 15;
        const int MAX_VALUE = 100000007;
        const int MAX_DEPTH = 4;
        const int MAX_B = 1;
        const int I_SIZE = 30;
        const int TOP = 70;
        public static readonly int[] H = { +1, -1, +0, +0, -1, +1, -1, +1 };
        public static readonly int[] C = { +0, +0, +1, -1, -1, +1, +1, -1 };
        public static readonly int[] AT = { 0, 5, 500, 50000, 5000000, MAX_VALUE};
        private int[,] coordsX = new int[n, m];
        private int[,] coordsY = new int[n, m];        
        private int[,] arr = new int[n, m];
        private int[] Qx = new int[n * m];
        private int[] Qy = new int[n * m];
        private int[,] d = new int[n, m];
       
        private PictureBox[,] state = new PictureBox[n, m];
        private Image X, O, E, X2, O2;
        private int player, chooseX, chooseY;
        private bool gameOver, computerIsThinking;
        private int clickedBt = 0;        

        public int max(int a, int b) { return a > b ? a : b; }
        public int min(int a, int b) { return a > b ? b : a; }
        public int abs(int x) { return x > 0 ? x : -x; }


        private int Calculate(int player, int[]sX, int[]sY, int depth)
        {
            int[,] tam = new int[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    tam[i, j] = arr[i, j];
            for (int i = depth + 1; i <= MAX_DEPTH; i++) tam[sX[i], sY[i]] = (i % 2 == 0 ? -1 : 1);
            int attack = 0;
            // Tính giá trị cho player.
            int[] cnt = new int[n + m];            
            int[] tmp = new int[n + m];
            cnt[0] = 0;
            for (int i = MAX_DEPTH; i > depth; i--)
                if (player == tam[sX[i], sY[i]])
                {                    
                    int x = sX[i];
                    int y = sY[i];
                    int doiThu = -player;
                    // Hàng ngang
                    int l = y, r = y;
                    while (l >= 0 && tam[x, l] != doiThu && y - l + 1 <= 5) l--;
                    while (r < m && tam[x, r] != doiThu && r - y + 1 <= 5) r++;
                    l++; r--;
                    int num = r  - l + 1;
                    // Xử lí 2 đầu cho đơn giản.
                    if (l == 0) tmp[0] = 0;
                    else tmp[0] = tam[x, l-1];
                    if (r == m - 1) tmp[num + 1] = 0;
                    else tmp[num + 1] = tam[x, r+1];

                    for (int k = 1; k <= num; k++) cnt[k] = 0;                    

                    for (int k = 1; l <= r; k++, l++)
                    {
                        tmp[k] = tam[x, l];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);                        
                    }
                    for (int d = 1; d <= num - 4; d++){
                        int c = d + 4;
                        if (tmp[d - 1] == 0 || tmp[c + 1] == 0) attack += AT[cnt[c] - cnt[d - 1]];                        
                    }                        

                    // Hàng dọc
                    l = x;  r = x;
                    while (l >= 0 && tam[l, y] != doiThu && x - l + 1 <= 5) l--;
                    while (r < n && tam[r, y] != doiThu && r - x + 1 <= 5) r++;
                    l++; r--;
                    num = r - l + 1;
                    if (l == 0) tmp[0] = 0;
                    else tmp[0] = tam[l - 1, y];
                    if (r == n - 1) tmp[num + 1] = 0;
                    else tmp[num + 1] = tam[r + 1, y];

                    for (int k = 1; k <= num; k++) cnt[k] = 0;                    

                    for (int k = 1; l <= r; k++, l++)
                    {
                        tmp[k] = tam[l, y];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);                        
                    }
                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        if (tmp[d - 1] == 0 || tmp[c + 1] == 0) attack += AT[cnt[c] - cnt[d - 1]];
                    }       
                    // Chéo chính                    
                    int x1 = x, y1 = y, x2 = x, y2 = y;
                    while (OK(x1, y1) && tam[x1, y1] != doiThu && x - x1 + 1 <= 5) { x1--; y1--; }
                    while (OK(x2, y2) && tam[x2, y2] != doiThu && x2 - x + 1 <= 5) { x2++; y2++; }                    

                    if (!OK(x1, y1)) tmp[0] = 0;
                    else tmp[0] = tam[x1, y1];
                    if (!OK(x2, y2)) tmp[num + 1] = 0;
                    else tmp[num + 1] = tam[x2, y2];
                    x1++; y1++; x2--; y2--;
                    num = x2 - x1 + 1;

                    for (int k = 1; k <= num; k++) cnt[k] = 0;      

                    for (int k = 1; x1 <= x2; k++, x1++, y1++)
                    {
                        tmp[k] = tam[x1, y1];
                        cnt[k] = cnt[k-1] + (tmp[k] == player ? 1 : 0);
                    }
                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        if (tmp[d - 1] == 0 || tmp[c + 1] == 0) attack += AT[cnt[c] - cnt[d - 1]];
                    } 
                    // Chéo phụ.
                    x1 = x; y1 = y; x2 = x; y2 = y;
                    while (OK(x1, y1) && tam[x1, y1] != doiThu && x - x1 + 1 <= 5) { x1--; y1++; }
                    while (OK(x2, y2) && tam[x2, y2] != doiThu && x2 - x + 1 <= 5) { x2++; y2--; }                    

                    if (!OK(x1, y1)) tmp[0] = 0;
                    else tmp[0] = tam[x1, y1];
                    if (!OK(x2, y2)) tmp[num + 1] = 0;
                    else tmp[num + 1] = tam[x2, y2];
                    x1++; y1--; x2--; y2++;
                    num = x2 - x1 + 1;

                    for (int k = 1; k <= num; k++) cnt[k] = 0;

                    for (int k = 1; x1 <= x2; k++, x1++, y1--)
                    {
                        tmp[k] = tam[x1, y1];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);
                    }
                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        if (tmp[d - 1] == 0 || tmp[c + 1] == 0) attack += AT[cnt[c] - cnt[d - 1]];
                    } 
                }
            return player * attack;
        }

        private void makeAllChilds(int player, int depth, int[] sX, int[] sY, ref int[][]xChilds, ref int[][]yChilds, ref int numChilds)
        {
            // Mỗi trạng thái chỉ là sự thay đổi của 4 ô thôi.
            // Gán tam  = arr, để đỡ lưu 
            int[,] tam = new int[n, m];
            for (int i = 0; i < n; i++ )
            {
                for (int j = 0; j < m; j++) tam[i, j] = arr[i, j];
            }
            // Sửa theo chuỗi trạng thái, chẵn thì là của O, lẻ thì là của X
            // Nếu chiều sâu là 4, thì rõ ràng là khỏi cần phải sửa gì hết. 
            // Nếu chiều sâu là 3 thì cần phải sửa 1 ô.
            for (int i = MAX_DEPTH; i > depth; i--) tam[sX[i], sY[i]] = (i % 2 == 0 ? -1 : 1);
            //BFS tìm con với độ rộng là MAX_B
            int L = 0, R = -1;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    d[i, j] = MAX_VALUE;
                    if (tam[i, j] != 0)
                    {
                        R++;
                        Qx[R] = i;
                        Qy[R] = j;
                        d[i, j] = 0;
                    }
                }
            while (L <= R)
            {
                int ux = Qx[L];
                int uy = Qy[L];
                L++;
                if (d[ux, uy] >= MAX_B) break;
                for (int i = 0; i < 8; i++)
                {
                    int vx = ux + H[i];
                    int vy = uy + C[i];
                    if (OK(vx, vy) && d[vx, vy] == MAX_VALUE)
                    {
                        d[vx, vy] = d[ux, uy] + 1;
                        R++;
                        Qx[R] = vx;
                        Qy[R] = vy;
                        numChilds++;
                    }
                }
            }
            // Sinh con.
            xChilds = new int[numChilds][];
            yChilds = new int[numChilds][];
            for (int i = 0; i < numChilds; i++)
            {
                xChilds[i] = new int[MAX_DEPTH + 1];
                yChilds[i] = new int[MAX_DEPTH + 1];
            }

            // Thừa kế từ cái cha.
            for (int i = 0; i < numChilds; i++)
            {                
                for (int j = MAX_DEPTH; j > depth; j--)
                {
                    xChilds[i][j] = sX[j];
                    yChilds[i][j] = sY[j];             
                }                
            }
            // Tự tạo riêng cho mình.
            int num = 0;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (tam[i, j] == 0 && d[i, j] <= MAX_B)
                    {
                        xChilds[num][depth] = i;
                        yChilds[num][depth] = j;                        
                        num++;
                    }
        }        

        private int AlphaBeta(int player, int[] sX, int[] sY, int depth, int alpha, int beta)
        {
            // Kiểm tra có phải nút lá hay không.
            int v1 = Calculate(-player, sX, sY, depth);
            int v2 = Calculate(player, sX, sY, depth);             
            if (abs(v1) >= MAX_VALUE) return v1;
            if (abs(v2) >= MAX_VALUE) return v2;
            // Nếu vét tới độ sâu tối đa. Thì tính luôn.
            if (depth == 0) return v1 + v2;
            // Nếu chưa tới độ sâu tối đa.
            // thì mình sẽ sinh con.
            int numChilds = 0;
            int[][] xChilds = new int[1][];
            int[][] yChilds = new int[1][];            
            
            makeAllChilds(player, depth, sX, sY, ref xChilds, ref yChilds, ref numChilds);           
            // Alpha Beta pruning
            if (player == 1)
            {                                
                int v = -MAX_VALUE;
                int id;
                for (int i = 0; i < numChilds; i++)
                {                    
                    int t = AlphaBeta(-1, xChilds[i], yChilds[i], depth - 1, alpha, beta);
                    if (v < t){
                        v = t;
                        id = i;
                    }
                    alpha = max(alpha, v);
                    if (alpha >= beta) break;                    
                }                
                return v;                
            }
            else
            {    
                int v = MAX_VALUE;
                int id = 0;
                for (int i = 0; i < numChilds; i++)
                {
                    int t = AlphaBeta(1, xChilds[i], yChilds[i], depth - 1, alpha, beta);
                    if (v > t){
                        v = t;
                        id = i;
                    }
                    beta = min(beta, v);
                    if (alpha >= beta) break;                     
                }

                if (depth == MAX_DEPTH)
                {
                    chooseX = xChilds[id][MAX_DEPTH];
                    chooseY = yChilds[id][MAX_DEPTH];
                }
                return v;
            }
        }
        private void Compute()
        {
            int[] sX = new int[1];
            int[] sY = new int[1];
            int v = AlphaBeta(-1, sX, sY, MAX_DEPTH, -MAX_VALUE, MAX_VALUE);
            //label1.Text = v.ToString();
            arr[chooseX, chooseY] = -1;
            state[chooseX, chooseY].Image = O2;
        }
        private void StateClick2(object sender, EventArgs e)
        {
            if (gameOver) return;
            if (computerIsThinking) return;
            computerIsThinking = true;
            PictureBox p = (PictureBox)sender;
            int x = 0, y = 0;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (state[i, j] == p)
                    {
                        x = i;
                        y = j;
                    }
            
            if (p.Image == E)
            {
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < m; j++)
                    {
                        if (state[i, j].Image == O2) state[i, j].Image = O;
                        if (state[i, j].Image == X2) state[i, j].Image = X;
                    }

                label2.Text = "Computer is thinking...";
                p.Image = X2;
                arr[x, y] = 1;
                int t = checkWin(arr);
                if (t == 1)
                {
                    WinGame(1);
                    return;
                }                
                timer1.Enabled = true;
                timer1.Start();
            }
            else
            {
                MessageBox.Show("You can't go here!");
                return;
            }
        }                 
        private void LoadState()
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    this.Controls.Add(state[i, j]);
                }
        }
        private void InitImage()
        {
            X = Image.FromFile("D:\\Game\\Caro\\X.png");
            O = Image.FromFile("D:\\Game\\Caro\\O.png");
            E = Image.FromFile("D:\\Game\\Caro\\E.png");
            X2 = Image.FromFile("D:\\Game\\Caro\\X2.png");
            O2 = Image.FromFile("D:\\Game\\Caro\\O2.png");
        }
        private void InitState()
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    arr[i, j] = 0;
                    state[i, j] = new PictureBox();
                    coordsX[i, j] = TOP + I_SIZE * (i - 1);
                    coordsY[i, j] = TOP + I_SIZE * (j - 1);
                    state[i, j].Location = new System.Drawing.Point(coordsX[i, j], coordsY[i, j]);
                    state[i, j].SizeMode = PictureBoxSizeMode.StretchImage;
                    state[i, j].Size = new Size(I_SIZE, I_SIZE);
                    state[i, j].Image = E;
                }
        }

        private bool OK(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < n && y < m);
        }
        private void WinGame(int x)
        {
            string t = (x == 1 ? "X" : "O");
            MessageBox.Show("Player " + t + " win!");
            gameOver = true;
        }
        private int nghich_dao(int x)
        {
            return -x;
        }
        private int checkWin(int[,] arr)
        {
            for (int i = 0; i < n; i++)
            {
                int sum = 0;
                if (arr[i, 0] != 0)
                {
                    sum = 1;
                }
                for (int j = 1; j < m; j++)
                {
                    if (arr[i, j] != 0)
                    {
                        if (arr[i, j] == arr[i, j - 1]) sum++;
                        else sum = 1;
                        if (sum >= 5)
                        {
                            bool b1 = OK(i, j - 5);
                            bool b2 = OK(i, j + 1);
                            if (!b1 || !b2) return arr[i, j];
                            if (arr[i, j - 5] != nghich_dao(arr[i, j]) || arr[i, j + 1] != nghich_dao(arr[i, j]))
                                return arr[i, j];
                        }
                    }
                    else sum = 0;
                }
            }

            for (int j = 0; j < m; j++)
            {
                int sum = 0;
                if (arr[0, j] != 0) sum = 1;
                for (int i = 1; i < n; i++)
                {
                    if (arr[i, j] != 0)
                    {
                        if (arr[i, j] == arr[i - 1, j]) sum++;
                        else sum = 1;
                        if (sum >= 5)
                        {
                            bool b1 = OK(i - 5, j);
                            bool b2 = OK(i + 1, j);
                            if (!b1 || !b2) return arr[i, j];
                            if (arr[i - 5, j] != nghich_dao(arr[i, j]) || arr[i + 1, j] != nghich_dao(arr[i, j]))
                                return arr[i, j];
                        }
                    }
                    else sum = 0;
                }
            }
            // Cheo 1
            for (int j = 0; j < m; j++)
            {
                int x = 0;
                int y = j;
                int sum = 0;
                if (arr[x, y] != 0) sum = 1;
                while (true)
                {
                    x += 1;
                    y -= 1;
                    if (!OK(x, y)) break;
                    if (arr[x, y] != 0)
                    {
                        if (arr[x - 1, y + 1] == arr[x, y]) sum++;
                        else sum = 1;
                        if (sum >= 5)
                        {
                            bool b1 = OK(x - 5, y + 5);
                            bool b2 = OK(x + 1, y - 1);
                            if (!b1 || !b2) return arr[x, y];
                            if (arr[x - 5, y + 5] != nghich_dao(arr[x, y]) || arr[x + 1, y - 1] != nghich_dao(arr[x, y]))
                            {
                                return arr[x, y];
                            }
                        }
                    }
                    else sum = 0;
                }
            }

            for (int i = 1; i < n; i++)
            {
                int x = i;
                int y = m - 1;
                int sum = 0;
                if (arr[x, y] != 0) sum = 1;
                while (true)
                {
                    x += 1;
                    y -= 1;
                    if (!OK(x, y)) break;
                    if (arr[x, y] != 0)
                    {
                        if (arr[x - 1, y + 1] == arr[x, y]) sum++;
                        else sum = 1;
                        if (sum >= 5)
                        {
                            bool b1 = OK(x - 5, y + 5);
                            bool b2 = OK(x + 1, y - 1);
                            if (!b1 || !b2) return arr[x, y];
                            if (arr[x - 5, y + 5] != nghich_dao(arr[x, y]) || arr[x + 1, y - 1] != nghich_dao(arr[x, y]))
                            {
                                return arr[x, y];
                            }
                        }
                    }
                    else
                        sum = 0;
                }
            }
            // Cheo 2
            for (int j = 0; j < m; j++)
            {
                int x = 0;
                int y = j;
                int sum = 0;
                if (arr[x, y] != 0) sum = 1;
                while (true)
                {
                    x += 1;
                    y += 1;
                    if (!OK(x, y)) break;
                    if (arr[x, y] != 0)
                    {
                        if (arr[x - 1, y - 1] == arr[x, y]) sum++;
                        else sum = 1;
                        if (sum >= 5)
                        {
                            bool b1 = OK(x - 5, y - 5);
                            bool b2 = OK(x + 1, y + 1);
                            if (!b1 || !b2) return arr[x, y];
                            if (arr[x - 5, y - 5] != nghich_dao(arr[x, y]) || arr[x + 1, y + 1] != nghich_dao(arr[x, y]))
                            {
                                return arr[x, y];
                            }
                        }
                    }
                    else
                        sum = 0;

                }
            }

            for (int i = 1; i < n; i++)
            {
                int x = i;
                int y = 0;
                int sum = 0;
                if (arr[x, y] != 0) sum = 1;
                while (true)
                {
                    x += 1;
                    y += 1;
                    if (!OK(x, y)) break;
                    if (arr[x, y] != 0)
                    {
                        if (arr[x - 1, y - 1] == arr[x, y]) sum++;
                        else sum = 1;
                        if (sum >= 5)
                        {
                            bool b1 = OK(x - 5, y - 5);
                            bool b2 = OK(x + 1, y + 1);
                            if (!b1 || !b2) return arr[x, y];
                            if (arr[x - 5, y - 5] != nghich_dao(arr[x, y]) || arr[x + 1, y + 1] != nghich_dao(arr[x, y]))
                            {
                                return arr[x, y];
                            }
                        }
                    }
                    else sum = 0;
                }
            }
            return 0;
        }
        private void StateClick(object sender, EventArgs e)
        {
            if (gameOver) return;
            PictureBox p = (PictureBox)sender;
            int x = 0, y = 0;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (state[i, j] == p)
                    {
                        x = j;
                        y = i;
                    }

            if (p.Image == E)
            {
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < m; j++)
                    {
                        if (state[i, j].Image == O2) state[i, j].Image = O;
                        if (state[i, j].Image == X2) state[i, j].Image = X;
                    }
                player++;
                if (player % 2 == 1)
                {
                    label2.Text = "O";
                    p.Image = X2;
                    arr[x, y] = 1;
                }
                else
                {
                    label2.Text = "X";
                    p.Image = O2;
                    arr[x, y] = -1;
                }
                int b = checkWin(arr);
                if (b != 0)
                {
                    WinGame(b);
                    return;
                }
            }
            else
            {
                MessageBox.Show("You can't go here!");
                return;
            }
        }
        private void InitGame()
        {
            InitImage();
            InitState();            
        }
        private void Form1_Load(object sender, EventArgs e)
        {                                
            InitGame();
            player = 0;            
            computerIsThinking = false;                     
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result;
            result = MessageBox.Show("Really?", "Exit", MessageBoxButtons.OKCancel);
            if (result == System.Windows.Forms.DialogResult.OK) this.Close();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Player: ";
            label2.Text = "You";
            clickedBt = 1;
            this.Controls.Remove(button1);
            this.Controls.Remove(button2);
            this.Controls.Remove(button3);
            LoadState();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    state[i, j].Click += new EventHandler(StateClick2);
                }            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clickedBt = 2;
            label1.Text = "Player: ";
            this.Controls.Remove(button1);
            this.Controls.Remove(button2);
            this.Controls.Remove(button3);
            LoadState();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    state[i, j].Click += new EventHandler(StateClick);
                }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();            
            Compute();      // AI here            
            computerIsThinking = false;
            label2.Text = "You";            
            int t = checkWin(arr);
            if (t == -1)
            {
                WinGame(-1);
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Controls.Add(button1);
            this.Controls.Add(button2);
            this.Controls.Add(button3);
            label1.Text = "";
            label2.Text = "";
            player = 0;
            gameOver = false;
            timer1.Enabled = false;
            timer1.Stop();
            for (int i=0; i<n; i++)
                for (int j = 0; j < m; j++)
                {
                    this.Controls.Remove(state[i, j]);
                    arr[i, j] = 0;
                    state[i, j].Image = E;
                    if (clickedBt == 1) state[i, j].Click -= new EventHandler(StateClick2);
                    else state[i, j].Click -= new EventHandler(StateClick);
                }
        }
    }
}