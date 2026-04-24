namespace Shara_s;

public partial class Jogo : ContentPage
{
    private const int GridSize = 10;
    private char[,] gridData = new char[GridSize, GridSize];
    private List<WordInfo> gameWords = new List<WordInfo>();
    private Button[,] gridButtons = new Button[GridSize, GridSize];
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string[] WordPool = new[]
    {
            "GATO","CÃO","PATO","LEÃO","URSO","PEIXE","COELHO","TIGRE","ELEFANTE","RAPOSA",
            "CAVALO","VACA","GALINHA","ABELHA","BORBOLETA","MACACO","RATO","OVELHA","SERPENTE","CORUJA",
             "PAPAGAIO","CAMELO","ZEBRA","HIPOPÓTAMO","GORILA","PANDA","CROCODILO","FLORESTA","MONTANHA","RIO", "OCEANO",
            "ILHA","DESERTO","CÉU","ESTRELA","NAVE","AVIÃO","CARRO","BICICLETA","FUTEBOL", "VOLEIBOL","BASQUETE","TENIS","NATAÇÃO",
            "CORRIDA","MÚSICA","DANÇA","PINTURA","LIVRO", "FILME", "TEATRO","FOTOGRAFIA","VIAGEM","CIDADE","PAÍS","MUNDO","AMIGO",
            "FAMÍLIA","AMOR", "FELICIDADE","TRISTEZA","RAIVA", "VIOLÃO", "PIANO","GUITARRA","BATERIA","VIOLINO","FLAUTA", "SAXOFONE"
        };
    private int wordsToPlace = 5;
    private List<string> lastSelectedWords = new List<string>();


    private Grid letterGrid;
    private CollectionView wordsListView;
    private Border winOverlay;

    public Jogo()
    {
        InitializeComponent();


        letterGrid = LetterGrid;
        wordsListView = WordsListContainer;
        winOverlay = WinOverlay;


        if (letterGrid.RowDefinitions.Count == 0 && letterGrid.ColumnDefinitions.Count == 0)
        {
            for (int i = 0; i < GridSize; i++)
            {
                letterGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                letterGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }
        }

        StartNewGame();
    }

    private void StartNewGame()
    {
        winOverlay.IsVisible = false;
        SetupLogic();
        RenderGrid();
        UpdateWordsList();
    }

    private void SetupLogic()
    {
        Array.Clear(gridData, 0, gridData.Length);

        var rnd = new Random();
        List<string> selection = null;
        int tries = 0;
        while (tries < 50)
        {
            tries++;
            var shuffled = WordPool.OrderBy(x => rnd.Next()).ToList();
            selection = shuffled.Take(Math.Min(wordsToPlace, shuffled.Count)).ToList();
            if (!selection.SequenceEqual(lastSelectedWords)) break;
        }
        lastSelectedWords = selection ?? WordPool.Take(wordsToPlace).ToList();
        gameWords = lastSelectedWords.Select(w => new WordInfo(w)).ToList();

        int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 } };

        foreach (var word in gameWords)
        {
            word.Cells.Clear();
            bool placed = false;
            int attempts = 0;
            while (!placed && attempts < 1000)
            {
                attempts++;
                int dir = rnd.Next(3);
                int r = rnd.Next(GridSize);
                int c = rnd.Next(GridSize);

                if (CanPlace(word.Text, r, c, directions[dir, 0], directions[dir, 1]))
                {
                    for (int i = 0; i < word.Text.Length; i++)
                    {
                        int row = r + i * directions[dir, 0];
                        int col = c + i * directions[dir, 1];
                        gridData[row, col] = word.Text[i];
                        word.Cells.Add((row, col));
                    }
                    placed = true;
                }
            }
        }

        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                if (gridData[r, c] == '\0')
                    gridData[r, c] = alphabet[rnd.Next(alphabet.Length)];
    }

    private bool CanPlace(string word, int r, int c, int dr, int dc)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int nr = r + i * dr;
            int nc = c + i * dc;
            if (nr < 0 || nr >= GridSize || nc < 0 || nc >= GridSize) return false;
            if (gridData[nr, nc] != '\0' && gridData[nr, nc] != word[i]) return false;
        }
        return true;
    }

    private void RenderGrid()
    {
        letterGrid.Children.Clear();

        for (int r = 0; r < GridSize; r++)
        {
            for (int c = 0; c < GridSize; c++)
            {
                var btn = new Button
                {
                    Text = gridData[r, c].ToString(),
                    FontSize = 18,
                    BackgroundColor = Color.FromArgb("#F0F9FF"),
                    TextColor = Colors.Black,
                    Padding = 2
                };
                int rr = r, cc = c;
                btn.Clicked += (s, e) => OnLetterClick(rr, cc);
                gridButtons[r, c] = btn;
                letterGrid.Add(btn, c, r);
            }
        }
    }

    private void OnLetterClick(int row, int col)
    {
        foreach (var word in gameWords)
        {
            if (!word.IsFound && word.Cells.Any(cell => cell.r == row && cell.c == col))
            {
                MarkWordFound(word);
            }
        }
        CheckWin();
    }

    private void MarkWordFound(WordInfo word)
    {
        word.IsFound = true;
        foreach (var (r, c) in word.Cells)
        {
            var btn = gridButtons[r, c];
            if (btn != null)
            {
                btn.BackgroundColor = Colors.LightGreen;
                btn.TextColor = Colors.White;
            }
        }
        UpdateWordsList();
    }

    private void UpdateWordsList()
    {
        var items = gameWords.Select(w => (w.IsFound ? "✓ " : "") + w.Text).ToList();
        wordsListView.ItemsSource = items;
    }

    private void OnNewGameClick(object sender, EventArgs e)
    {
        StartNewGame();
    }

    private void CheckWin()
    {
        if (gameWords.All(w => w.IsFound))
            winOverlay.IsVisible = true;
    }
}

public class WordInfo
{
    public string Text { get; set; }
    public bool IsFound { get; set; }
    public List<(int r, int c)> Cells { get; set; } = new List<(int, int)>();
    public WordInfo(string t) => Text = t;
}
