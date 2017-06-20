using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

namespace Caro_AlphaBeta
{

    public partial class Form1 : Form
    {
        public class Pair<T, U>
        {
            public Pair()
            {
            }

            public Pair(T first, U second)
            {
                this.First = first;
                this.Second = second;
            }

            public T First { get; set; }
            public U Second { get; set; }
        };
        public Form1()
        {
            InitializeComponent();
        }

        Thread _timeThread;
        Thread _compute;

        const int n = 15;
        const int m = 15;
        const int oo = 500050000;
        const int MAX_DEPTH = 6;
        const int MAX_B = 1;
        const int I_SIZE = 30;
        const int TOP = 70;
        public static readonly int[] H = { +1, -1, +0, +0, -1, +1, -1, +1 };
        public static readonly int[] C = { +0, +0, +1, -1, -1, +1, +1, -1 };        
        public static readonly int[] AT = { 0, 10, 100, 1000, 3000, 500000000};
        public static readonly int[] MUL = { 0, 1, 3, 1, 100, 1 };
        private int[,] coordsX = new int[n, m];
        private int[,] coordsY = new int[n, m];        
        private int[,] arr = new int[n, m];
        private int[] Qx = new int[n * m];
        private int[] Qy = new int[n * m];
        private int[,] d = new int[n, m];
        private bool[,] b = new bool[n, m];
        private bool[,] visited = new bool[n, m];
        int[] _priority = new int[6];            
        private int wingame;
       
        private PictureBox[,] state = new PictureBox[n, m];
        private Image X, O, E, X2, O2;        
        
        private bool gameOver, computerIsThinking;
        private int clickedBt = 0;
        private int giatri;
        Pair<int, int> _next;        
        Pair<int, int>[] played = new Pair<int,int>[n * m];
        private int _nPlayed, _maximum;
        private int _value;
        private int _timeOut;
        public int max(int a, int b) { return a > b ? a : b; }
        public int min(int a, int b) { return a > b ? b : a; }
        public int abs(int x) { return x > 0 ? x : -x; }


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
            X = Image.FromFile(@"src/X.png");
            O = Image.FromFile(@"src/O.png");
            E = Image.FromFile(@"src/E.png");
            X2 = Image.FromFile(@"src/X2.png");
            O2 = Image.FromFile(@"src/O2.png");
        }
        private void InitState()
        {
            time.Text = "";

            _priority[5] = +oo;
            _priority[4] = +oo - 1;
            _nPlayed = _maximum = 0;
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
        private int nghich_dao(int x) { return -x; }        
        private void InitGame()
        {
            InitImage();
            InitState();            
        }
        private void Form1_Load(object sender, EventArgs e)
        {                                
            InitGame();            
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
            clickedBt = 1;
            current.Text = "Player: ";
            thongbao.Text = "You";            
            this.Controls.Remove(playGame);            
            this.Controls.Remove(Exit);
            LoadState();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    state[i, j].Click += new EventHandler(StateClick2);                
        }
              

        private void button4_Click(object sender, EventArgs e)
        {
            this.Controls.Add(playGame);            
            this.Controls.Add(Exit);
            current.Text = "";
            thongbao.Text = "";            
            gameOver = false;
            
            for (int i=0; i<n; i++)
                for (int j = 0; j < m; j++)
                {
                    visited[i, j] = false;
                    this.Controls.Remove(state[i, j]);                    
                    state[i, j].Image = E;
                    if (clickedBt == 1) state[i, j].Click -= new EventHandler(StateClick2);                    
                }
        }
        private void makeAllChilds(ref Pair<int, int>[] childs, ref int nums)
        {            
            int L = 0, R = -1;
            for (int i = 0; i < _nPlayed; i++)
            {
                ++R;
                Qx[R] = played[i].First;
                Qy[R] = played[i].Second;                
            }
            int First = R + 1;
            nums = 0;
            while (L < First)
            {
                int ux = Qx[L]; int uy = Qy[L]; L++;                                
                for (int i = 0; i < 8; i++)
                {
                    int vx = ux + H[i]; int vy = uy + C[i];
                    if (OK(vx, vy) && visited[vx, vy] == false)
                    {                        
                        ++R;
                        Qx[R] = vx; Qy[R] = vy;
                        visited[vx, vy] = true;
                        nums++;                                                
                    }
                }
            }
            
            {
                childs = new Pair<int, int>[nums];
                int k = 0;
                for (int i = First; i <= R; i++)
                {
                    childs[k] = new Pair<int, int>(Qx[i], Qy[i]);                    
                    k++;
                }
            }            
            for (int i = First; i <= R; i++)
                visited[Qx[i], Qy[i]] = false;
        }
        private bool leaf(int player)
        {            
            Pair<int, int> curr;
            curr = played[_nPlayed - 1];            
            // kiểm tra xem có thỏa mãn thắng không, nếu có thì _value = +oo
            _value = calculateWin(player, curr);
            return _value == player * oo;            
        }
        //calculate trả về giá trị 0 nếu player không có khả năng thắng nếu đi tiếp bước nữa.
        //trả về +oo nếu thắng
        //tính toán giá trị cần trả về
        private bool OkAndNull(int x, int y) { return OK(x, y) && arr[x, y] == 0; }
        private int calculateWin(int player, Pair<int, int> curr)
        {
            wingame = 0;

            // đi lên, đi xuống, x - 1, y; x + 1, y 
            int xT = curr.First - 1, yT = curr.Second;
            while (OK(xT, yT) && arr[xT, yT] == player) xT--; xT++;
            int xB = curr.First + 1, yB = curr.Second;
            while (OK(xB, yB) && arr[xB, yB] == player) xB++; xB--;
            if (xB - xT + 1 == 5)
            {
                if (!OK(xB + 1, yB) || !OK(xT - 1, yT)) { wingame = player; return player * _priority[0]; }
                if (arr[xB + 1, yB] == 0 || arr[xT - 1, yT] == 0) { wingame = player; return player * _priority[0]; }
            }
            
            //đi trái, đi phải
            int xL = curr.First, yL = curr.Second - 1;
            while (OK(xL, yL) && arr[xL, yL] == player) yL--; yL++;
            int xR = curr.First, yR = curr.Second + 1;
            while (OK(xR, yR) && arr[xR, yR] == player) yR++; yR--;
            if (yR - yL + 1 == 5)
            {
                if (!OK(xL, yL - 1) || !OK(xR, yR + 1)) { wingame = player; return player * _priority[0]; }
                if (arr[xL, yL - 1] == 0 || arr[xR, yR + 1] == 0) { wingame = player; return player * _priority[0]; }
            }

            //đi chéo chính
            int xCT = curr.First - 1, yCT = curr.Second - 1;
            while (OK(xCT, yCT) && arr[xCT, yCT] == player) { xCT--; yCT--; } xCT++; yCT++;
            int xCB = curr.First + 1, yCB = curr.Second + 1;
            while (OK(xCB, yCB) && arr[xCB, yCB] == player) { xCB++; yCB++; } xCB--; yCB--;
            if (xCB - xCT + 1 == 5)
            {
                if (!OK(xCT - 1, yCT - 1) || !OK(xCB + 1, yCB + 1)) { wingame = player; return player * _priority[0]; }
                if (arr[xCT - 1, yCT - 1] == 0 || arr[xCB + 1, yCB + 1] == 0) { wingame = player; return player * _priority[0]; }
            }

            //đi chéo phụ, x-1, y+1; x+1, y-1
            int xPT = curr.First - 1, yPT = curr.Second + 1;
            while (OK(xPT, yPT) && arr[xPT, yPT] == player) { xPT--; yPT++; } xPT++; yPT--;
            int xPB = curr.First + 1, yPB = curr.Second - 1;
            while (OK(xPB, yPB) && arr[xPB, yPB] == player) { xPB++; yPB--; } xPB--; yPB++;
            if (xPB - xPT + 1 == 5)
            {
                if (!OK(xPT - 1, yPT + 1) || !OK(xPB + 1, yPB - 1)) { wingame = player; return player * _priority[0]; }
                if (arr[xPT - 1, yPT + 1] == 0 || arr[xPB + 1, yPB - 1] == 0) { wingame = player; return player * _priority[0]; }
            }

            return 0;
        }
        private int calculateNormal(int player)
        {
            int heuristic = 0;
            int choosing = (player == 1 ? 0 : 1);
            // Tính giá trị cho player.
            int[] cnt = new int[n + m];
            int[] tmp = new int[n + m];
            cnt[0] = 0;            
            int numWay = 0;
            for (int i = _maximum; i < _nPlayed; i++) if (i % 2 == choosing)
                {
                    int x = played[i].First, y = played[i].Second;
                    int doiThu = -player;
                    // Hàng ngang
                    int l = y, r = y;
                    while (l >= 0 && arr[x, l] != doiThu && y - l + 1 <= 5) l--;
                    while (r < m && arr[x, r] != doiThu && r - y + 1 <= 5) r++;
                    l++; r--;
                    int num = r - l + 1;
                    // Xử lí 2 đầu cho đơn giản.
                    if (l == 0) tmp[0] = 0;
                    else tmp[0] = arr[x, l - 1];
                    if (r == m - 1) tmp[num + 1] = 0;
                    else tmp[num + 1] = arr[x, r + 1];

                    for (int k = 1; k <= num; k++) cnt[k] = 0;

                    for (int k = 1; l <= r; k++, l++)
                    {
                        tmp[k] = arr[x, l];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);
                    }

                    bool ok = false;

                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        // Chặn 1 đầu
                        int count = cnt[c] - cnt[d - 1];
                        if (count >= 3) ok = true;
                        if (tmp[d - 1] == 0 && tmp[c + 1] != 0) heuristic += AT[count];
                        if (tmp[d - 1] != 0 && tmp[c + 1] == 0) heuristic += AT[count];
                        // Không chặn đầu nào                        
                        if (tmp[d - 1] == 0 && tmp[c + 1] == 0) heuristic += MUL[count] * AT[count];
                    }
                    if (ok) numWay++;
                    // Hàng dọc
                    l = x; r = x;
                    while (l >= 0 && arr[l, y] != doiThu && x - l + 1 <= 5) l--;
                    while (r < n && arr[r, y] != doiThu && r - x + 1 <= 5) r++;
                    l++; r--;
                    num = r - l + 1;
                    if (l == 0) tmp[0] = 0;
                    else tmp[0] = arr[l - 1, y];
                    if (r == n - 1) tmp[num + 1] = 0;
                    else tmp[num + 1] = arr[r + 1, y];

                    for (int k = 1; k <= num; k++) cnt[k] = 0;

                    for (int k = 1; l <= r; k++, l++)
                    {
                        tmp[k] = arr[l, y];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);
                    }

                    ok = false;

                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        // Chặn 1 đầu
                        int count = cnt[c] - cnt[d - 1];
                        if (count >= 3) ok = true;
                        if (tmp[d - 1] == 0 && tmp[c + 1] != 0) heuristic += AT[count];
                        if (tmp[d - 1] != 0 && tmp[c + 1] == 0) heuristic += AT[count];
                        // Không chặn đầu nào                        
                        if (tmp[d - 1] == 0 && tmp[c + 1] == 0) heuristic += MUL[count] * AT[count];
                    }
                    if (ok) numWay++;
                    // Chéo chính                    
                    int x1 = x, y1 = y, x2 = x, y2 = y;
                    while (OK(x1, y1) && arr[x1, y1] != doiThu && x - x1 + 1 <= 5) { x1--; y1--; }
                    while (OK(x2, y2) && arr[x2, y2] != doiThu && x2 - x + 1 <= 5) { x2++; y2++; }

                    if (!OK(x1, y1)) tmp[0] = 0;
                    else tmp[0] = arr[x1, y1];
                    if (!OK(x2, y2)) tmp[num + 1] = 0;
                    else tmp[num + 1] = arr[x2, y2];
                    x1++; y1++; x2--; y2--;
                    num = x2 - x1 + 1;

                    for (int k = 1; k <= num; k++) cnt[k] = 0;

                    for (int k = 1; x1 <= x2; k++, x1++, y1++)
                    {
                        tmp[k] = arr[x1, y1];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);
                    }

                    ok = false;

                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        int count = cnt[c] - cnt[d - 1];
                        if (count >= 3) ok = true;
                        if (tmp[d - 1] == 0 && tmp[c + 1] != 0) heuristic += AT[count];
                        if (tmp[d - 1] != 0 && tmp[c + 1] == 0) heuristic += AT[count];
                        // Không chặn đầu nào                        
                        if (tmp[d - 1] == 0 && tmp[c + 1] == 0) heuristic += MUL[count] * AT[count];
                    }
                    if (ok) numWay++;
                    // Chéo phụ.
                    x1 = x; y1 = y; x2 = x; y2 = y;
                    while (OK(x1, y1) && arr[x1, y1] != doiThu && x - x1 + 1 <= 5) { x1--; y1++; }
                    while (OK(x2, y2) && arr[x2, y2] != doiThu && x2 - x + 1 <= 5) { x2++; y2--; }

                    if (!OK(x1, y1)) tmp[0] = 0;
                    else tmp[0] = arr[x1, y1];
                    if (!OK(x2, y2)) tmp[num + 1] = 0;
                    else tmp[num + 1] = arr[x2, y2];
                    x1++; y1--; x2--; y2++;
                    num = x2 - x1 + 1;

                    for (int k = 1; k <= num; k++) cnt[k] = 0;

                    for (int k = 1; x1 <= x2; k++, x1++, y1--)
                    {
                        tmp[k] = arr[x1, y1];
                        cnt[k] = cnt[k - 1] + (tmp[k] == player ? 1 : 0);
                    }

                    ok = false;

                    for (int d = 1; d <= num - 4; d++)
                    {
                        int c = d + 4;
                        int count = cnt[c] - cnt[d - 1];
                        if (count >= 3) ok = true;
                        if (tmp[d - 1] == 0 && tmp[c + 1] != 0) heuristic += AT[count];
                        if (tmp[d - 1] != 0 && tmp[c + 1] == 0) heuristic += AT[count];
                        // Không chặn đầu nào                        
                        if (tmp[d - 1] == 0 && tmp[c + 1] == 0) heuristic += MUL[count] * AT[count];
                    }
                    if (ok) numWay++;
                }
            if (numWay == 0) numWay = 1;
            return player * numWay * heuristic;            
        }        
        // giả lập 1 bước nữa, player có thắng nỗi k?
        private int simulate(int player)
        {
            return 0;            
        }
        private void Swap(ref int a, ref int b)
        {
            int t = a;
            a = b;
            b = t;
        }
        private void QSort(ref int[] gt, ref Pair<int, int>[] childs, int l, int r)
        {
            int i = l, j = r, x = gt[(l + r) / 2];
            while (i < j)
            {
                while (gt[i] < x) i++;
                while (gt[j] > x) j--;
                if (i <= j)
                {
                    Swap(ref gt[i], ref gt[j]);
                    
                    int a = childs[i].First;
                    int b = childs[j].First;
                    Swap(ref a, ref b);
                    childs[i].First = a;
                    childs[j].First = b;

                    a = childs[i].Second;
                    b = childs[j].Second;
                    Swap(ref a, ref b);
                    childs[i].Second = a;
                    childs[j].Second = b;

                    i++;
                    j--;
                }
            }
            if (i < r) QSort(ref gt, ref childs, i, r);
            if (j > l) QSort(ref gt, ref childs, l, j);
        }
        private int AlphaBeta(int player, int depth, int alpha, int beta, int max_depth)
        {            
            if (leaf(-player)) { return _value; }
            if (depth == 0)
            {
                return calculateNormal(1) + calculateNormal(-1);
            }
            // sinh con và sắp xếp các con theo thứ tự
            // chọn ngẫu nhiên vùng được chọn để gần.

            int chooseRegion = (player == -1 ? 1 : 0);
            Pair<int, int>[] childs = new Pair<int, int>[1];            
            int nums = 0;
            makeAllChilds(ref childs, ref nums);
            // tính khoảng cách mahatan nhỏ nhất với vùng của đối phương nhắm phòng thủ tốt hơn.
            int[] gt = new int[nums];
            for (int i = 0; i < nums; i++)
            {
                gt[i] = 0;
                for (int j = 0; j < _nPlayed; j++) if (j % 2 == chooseRegion)
                {
                    gt[i] += abs(childs[i].First - played[j].First) + 
                             abs(childs[i].Second - played[j].Second);
                }                
            }
            QSort(ref gt, ref childs, 0, nums - 1);            

            // Alpha Beta pruning
            if (player == 1)
            {
                int v = -oo;
                for (int i = 0; i < nums; i++)
                {
                    int x = childs[i].First, y = childs[i].Second;
                    played[_nPlayed++] = new Pair<int, int>(x, y);
                    arr[x, y] = player;
                    visited[x, y] = true;
                    int t = AlphaBeta(-1, depth - 1, alpha, beta, max_depth);
                    _nPlayed--;
                    visited[x, y] = false;
                    arr[x, y] = 0;
                    v = max(v, t);
                    alpha = max(alpha, v);
                    if (alpha >= beta) break;
                }
                return v;
            }
            else
            {
                int v = +oo;
                int _id = 0;
                for (int i = 0; i < nums; i++)
                {
                    int x = childs[i].First, y = childs[i].Second;
                    played[_nPlayed++] = new Pair<int, int>(x, y);
                    visited[x, y] = true;
                    arr[x, y] = player;
                    int t = AlphaBeta(1, depth - 1, alpha, beta, max_depth);
                    _nPlayed--;
                    visited[x, y] = false;
                    arr[x, y] = 0;
                    if (v > t) { _id = i; v = t; }
                    beta = min(beta, v);
                    if (alpha >= beta) break;
                }
                if (depth == max_depth)
                {
             //       MessageBox.Show(v.ToString());
                    bool ok = false;
                    // đi qua tất cả con, cái nào win thì lấy luôn.
                    if (v == player * oo)
                    {
                        for (int i = 0; i < nums; i++)
                        {
                            if (calculateWin(player, childs[i]) == v)
                            {
                                _next = childs[i];
                                ok = true;
                                break;
                            }
                        }
                    }
                    if (!ok) _next = childs[_id];
                }                
                giatri = v;
                return v;
            }
        }
        private void Compute()
        {            
            AlphaBeta(-1, 4, -oo, +oo, 4);
            _timeThread.Abort();            

            state[_next.First, _next.Second].Image = O2;
            played[_nPlayed++] = new Pair<int, int>(_next.First, _next.Second);
            visited[_next.First, _next.Second] = true;
            arr[_next.First, _next.Second] = -1;
            _maximum++;

            computerIsThinking = false;
            thongbao.BeginInvoke((Action)(() =>
            {
                thongbao.Text = "You";
            }));            
        }
        private void StateClick2(object sender, EventArgs e)
        {
            if (_nPlayed % 2 == 1)
            {
                MessageBox.Show("You are cheating... :)) ");
                return; 
            }
            if (gameOver) return;
            if (computerIsThinking) return;

            // People go!!!
            PictureBox p = (PictureBox)sender;
            Pair<int, int> people = new Pair<int,int>();            
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (state[i, j] == p) { people.First = i; people.Second = j; }

            if (p.Image == E)
            {                
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < m; j++)
                    {
                        if (state[i, j].Image == O2) state[i, j].Image = O;
                        if (state[i, j].Image == X2) state[i, j].Image = X;
                    }

                played[_nPlayed++] = new Pair<int, int>(people.First, people.Second);
                visited[people.First, people.Second] = true;
                arr[people.First, people.Second] = 1;
                _maximum = _nPlayed;

                p.Image = X2;
                calculateWin(1, people);
                if (wingame == 1)  { WinGame(1); return; }
                

                // máy tính chuẩn bị chơi
                thongbao.Text = "Computer is thinking...";
                computerIsThinking = true;
                
                timer1.Interval = 100;
                timer1.Enabled = true;
                timer1.Start();
            }
            else
            {
                MessageBox.Show("You can't go here!");
                return;
            }        
        }    
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();
            //Computer go!!!            

            _next = new Pair<int, int>(-1, 0);
            
            _timeOut = 15;            
            _timeThread = new Thread(decTime);
            _compute = new Thread(Compute);

            _timeThread.Start();
            _compute.Start();

            _timeThread.Join();
            _compute.Join();

            calculateWin(-1, _next);
            if (wingame == -1) { WinGame(-1); return; }            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int nums = _nPlayed;
            state[played[nums - 1].First, played[nums - 1].Second].Image = E;
            visited[played[nums - 1].First, played[nums - 1].Second] = false;
            arr[played[nums - 1].First, played[nums - 1].Second] = 0;
            _nPlayed--;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_nPlayed == _maximum)
            {
                MessageBox.Show("No way!");
                return;
            }
            _nPlayed++;
            int nums = _nPlayed;
            if (_nPlayed % 2 == 0) state[played[nums - 1].First, played[nums - 1].Second].Image = O;
            else state[played[nums - 1].First, played[nums - 1].Second].Image = X;
            visited[played[nums - 1].First, played[nums - 1].Second] = true;
            arr[played[nums - 1].First, played[nums - 1].Second] = (_nPlayed % 2 == 0 ? -1 : 1);
        }
        private void decTime()
        {
            time.BeginInvoke((Action)(() =>
            {
                if (_timeOut >= 0) time.Text = "Time out: " + (_timeOut / 60).ToString() + @"'" + (_timeOut % 60).ToString() + @"''";
            }));            
            while (_timeOut >= 0)
            {                                
                 if (_timeOut >= 0) time.Text = "Time out: " + (_timeOut / 60).ToString() + @"'" + (_timeOut % 60).ToString() + @"''";                         
                _timeOut--;                
                Thread.Sleep(1000);
            }    
        
            _compute.Abort();
                                    
            while (_maximum < _nPlayed)
            {
                int x = played[_nPlayed - 1].First, y = played[_nPlayed - 1].Second;
                visited[x, y] = false;
                arr[x, y] = 0;
                _nPlayed--;
            }
            if (_next.First == -1)
            {
                AlphaBeta(-1, 2, -oo, +oo, 2);
                state[_next.First, _next.Second].Image = O2;
                played[_nPlayed++] = new Pair<int, int>(_next.First, _next.Second);
                visited[_next.First, _next.Second] = true;
                arr[_next.First, _next.Second] = -1;
                _maximum++;

                computerIsThinking = false;
                thongbao.BeginInvoke((Action)(() =>
                {
                    thongbao.Text = "You";
                }));
                _timeThread.Abort();
            }
        }
    }

}




/*
            int sum = 0;            
            int count1 = 0;
            
            int xT = curr.First - 1, yT = curr.Second;
            while (OK(xT, yT) && arr[xT, yT] == player) xT--; xT++;
            int xB = curr.First + 1, yB = curr.Second;
            while (OK(xB, yB) && arr[xB, yB] == player) xB++; xB--;            

            //đi trái, đi phải
            int xL = curr.First, yL = curr.Second - 1;
            while (OK(xL, yL) && arr[xL, yL] == player) yL--; yL++;
            int xR = curr.First, yR = curr.Second + 1;
            while (OK(xR, yR) && arr[xR, yR] == player) yR++; yR--;

            //đi chéo chính
            int xCT = curr.First - 1, yCT = curr.Second - 1;
            while (OK(xCT, yCT) && arr[xCT, yCT] == player) { xCT--; yCT--; } xCT++; yCT++;
            int xCB = curr.First + 1, yCB = curr.Second + 1;
            while (OK(xCB, yCB) && arr[xCB, yCB] == player) { xCB++; yCB++; } xCB--; yCB--;

            //đi chéo phụ, x-1, y+1; x+1, y-1
            int xPT = curr.First - 1, yPT = curr.Second + 1;
            while (OK(xPT, yPT) && arr[xPT, yPT] == player) { xPT--; yPT++; } xPT++; yPT--;
            int xPB = curr.First + 1, yPB = curr.Second - 1;
            while (OK(xPB, yPB) && arr[xPB, yPB] == player) { xPB++; yPB--; } xPB--; yPB++;

            // >=2 bộ 3 (không bị chặn) hoặc bộ 4( bị chặn 1 đầu)
            // đi lên, xuống            
            if (xB - xT + 1 == 4)
            {
                if ((!OK(xB + 1, yB) || (arr[xB + 1, yB] != 0)) && OkAndNull(xT - 1, yT) && OkAndNull(xT - 2, yT)) count1++;
                if ((!OK(xT - 1, yT) || (arr[xT - 1, yT] != 0)) && OkAndNull(xB + 1, yB) && OkAndNull(xB + 2, yB)) count1++;
                // khi này là 4 con không bị chặn, thắng rồi đó.
                if (OkAndNull(xB + 1, yB) && OkAndNull(xT - 1, yT)) count1++;
            }
            // đi qua trái, phải
            if (yR - yL + 1 == 4)
            {
                if ((!OK(xR, yR + 1) || (arr[xR, yR + 1] != 0)) && OkAndNull(xL, yL - 1) && OkAndNull(xL, yL - 2)) count1++;
                if ((!OK(xL, yL - 1) || (arr[xL, yL - 1] != 0)) && OkAndNull(xR, yR + 1) && OkAndNull(xR, yR + 2)) count1++;

                if (OkAndNull(xR, yR + 1) && OkAndNull(xL, yL - 1)) count1++;
            }
            // chéo chính
            if (xCB - xCT + 1 == 4)
            {
                if ((!OK(xCT - 1, yCT - 1) || (arr[xCT - 1, yCT - 1] == 0)) && OkAndNull(xCB + 1, yCB + 1) && OkAndNull(xCB + 2, yCB + 2)) count1++;
                if ((!OK(xCB + 1, yCB + 1) || (arr[xCB + 1, yCB + 1] != 0)) && OkAndNull(xCT - 1, yCT - 1) && OkAndNull(xCT - 2, yCT - 2)) count1++;

                if (OkAndNull(xCT - 1, yCT - 1) && OkAndNull(xCB + 1, yCB + 1)) count1++;
            }
            //chéo phụ
            if (xPB - xPT + 1 == 4)
            {
                if ((!OK(xPT - 1, yPT + 1) || (arr[xPT - 1, yPT + 1] == 0)) && OkAndNull(xPB + 1, yPB - 1) && OkAndNull(xPB + 2, yPB - 2)) count1++;
                if ((!OK(xPB + 1, yPB - 1) || (arr[xPB + 1, yPB - 1] != 0)) && OkAndNull(xPT - 1, yPT + 1) && OkAndNull(xPT - 2, yPT + 2)) count1++;

                if (OkAndNull(xPT - 1, yPT + 1) && OkAndNull(xPB + 1, yPB - 1)) count1++;
            }

            // >=2 bộ 3 không bị chặn
            if (xB - xT + 1 == 3)
            {
                if (OkAndNull(xB + 1, yB) && OkAndNull(xT - 1, yT)) count1++;
            }
            // đi qua trái, phải
            if (yR - yL + 1 == 3)
            {
                if (OkAndNull(xR, yR + 1) && OkAndNull(xL, yL - 1)) count1++;
            }
            // chéo chính
            if (xCB - xCT + 1 == 3)
            {
                if (OkAndNull(xCT - 1, yCT - 1) && OkAndNull(xCB + 1, yCB + 1)) count1++;
            }
            //chéo phụ
            if (xPB - xPT + 1 == 3)
            {
                if (OkAndNull(xPT - 1, yPT + 1) && OkAndNull(xPB + 1, yPB - 1)) count1++;
            }

            if (count1 >= 2) sum += 500;
            if (count1 == 1) sum += 100;
              
            // new heuristic
            count1 = 0;
            xT = curr.First - 1; yT = curr.Second;
            while (OK(xT, yT) && arr[xT, yT] != -player)
            {
                if (arr[xT, yT] == player) count1++;
                xT--;                
            }
            xT++;
            sum += count1 * count1;

            count1 = 0;
            xB = curr.First + 1; yB = curr.Second;
            while (OK(xB, yB) && arr[xB, yB] != -player)
            {
                if (arr[xB, yB] == player) count1++;
                xB++;                
            }
            xB--;
            sum += count1 * count1;

            //đi trái, đi phải
            count1 = 0;
            xL = curr.First; yL = curr.Second - 1;
            while (OK(xL, yL) && arr[xL, yL] != -player)
            {
                if (arr[xL, yL] == player) count1++;
                yL--;
            }
            yL++;
            sum += count1 * count1;

            count1 = 0;
            xR = curr.First; yR = curr.Second + 1;
            while (OK(xR, yR) && arr[xR, yR] != -player)
            {
                if (arr[xR, yR] == player) count1++;
                yR++;
            }
            yR--;
            sum += count1 * count1;

            //đi chéo chính
            count1 = 0;
            xCT = curr.First - 1; yCT = curr.Second - 1;
            while (OK(xCT, yCT) && arr[xCT, yCT] != -player) {
                if (arr[xCT, yCT] == player) count1++;
                xCT--; yCT--; 
            } 
            xCT++; yCT++;
            sum += count1 * count1;

            count1 = 0;
            xCB = curr.First + 1; yCB = curr.Second + 1;
            while (OK(xCB, yCB) && arr[xCB, yCB] != -player) {
                if (arr[xCB, yCB] == player) count1++;
                xCB++; yCB++; 
            } 
            xCB--; yCB--;
            sum += count1 * count1;

            count1 = 0;
            //đi chéo phụ, x-1, y+1; x+1, y-1
            xPT = curr.First - 1; yPT = curr.Second + 1;
            while (OK(xPT, yPT) && arr[xPT, yPT] != -player) {
                if (arr[xPT, yPT] == player) count1++;
                xPT--; yPT++; 
            } 
            xPT++; yPT--;
            sum += count1 * count1;

            xPB = curr.First + 1; yPB = curr.Second - 1;
            while (OK(xPB, yPB) && arr[xPB, yPB] != -player) {
                if (arr[xPB, yPB] == player) count1++;
                xPB++; yPB--; 
            } 
            xPB--; yPB++;
            sum += count1 * count1;
            */