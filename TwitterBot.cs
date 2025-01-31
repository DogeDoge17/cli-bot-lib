using Quill;
using Quill.Pages;

namespace cli_bot;

public class TwitterBot
{
    public delegate void RunBot(ComposePage composer, string[] args);
    public delegate void PreRun(string[] args);

    public event RunBot runAction;
    public event PreRun preAction;

    public double timer;

    public double interval;

    public int timeout = 250;

    public float Progress { get { return 1 - (float)(timer / interval); } }

    public string DisplayName { get; set; }

    public bool CustomTimer { get; set; }

    private ComposePage _page;
    public TwitterClient client;

    internal Thread _runThread;

    internal byte _loginState;
    internal DateTime _loginStart;
    internal DateTime _runStart;

    internal long ticks;

    public TwitterBot(TimeSpan interval) : this(interval.TotalSeconds) { }

    public TwitterBot(double intervalS)
    {
        interval = intervalS;
    }

    public void Start(string[]? argv = null)
    {
        Console.Clear();
        long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _loginStart = DateTimeOffset.UtcNow.LocalDateTime;
        double deltaTime = 0.0f;

        string[] creds = null;
        if (File.Exists(Path.Assembly / "login.txt"))
            creds = File.ReadAllLines(Path.Assembly / "login.txt").Select(str => str.Trim()).ToArray();
        else
        {
            Console.WriteLine("Please create and fill out a login file.");
            return;
        }

        DriverCreation.SetBrowserType(BrowserType.Firefox);
        DriverCreation.SuppressInitialDiagnosticInformation = true;
        DriverCreation.logLevel = LogLevel.None;
        client = new();

        ThreadStart threadStart = new ThreadStart(() =>
        {
            _loginState = 1;
            Output.WriteLine("Beginning login");
            client.Login(creds[0], "", creds[1]);
            Output.WriteLine($"Logged in as {creds[0]}");
            _loginState = 2;
            Output.WriteLine("Creating compose page");
            _page = client.CreateCompose();
            _loginState = 3;
            Output.WriteLine("Finished logging in.");
            creds = null;
            Console.Clear();
        });
        var thr = new Thread(threadStart) { IsBackground = true, Name = DisplayName + " login" };
        thr.Start();


        while (true)
        {
            deltaTime = (double)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTime) / 1000.0;
            lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (_loginState < 3)
                goto end;


            timer -= deltaTime;

            if (timer <= 0)
            {

                try
                {
                    //purposefully not multithreaded as preactions should be efficient 
                    if (preAction != null)
                        preAction.Invoke(argv);
                }
                catch (Exception e)
                {
                    Output.WriteLine(e.ToString());
                }

                ThreadStart runThreadStart = new ThreadStart(() =>
                {
                    try
                    {
                        if (runAction != null)
                            runAction.Invoke(_page, argv);
                        Console.Clear();
                    }
                    catch (Exception e)
                    {
                        Output.WriteLine(e.ToString());
                    }
                });

                _runThread = new Thread(runThreadStart) { IsBackground = true, Name = DisplayName + " login" };
                _runThread.Start();

                if (!CustomTimer)
                    SetTimer(TimeSpan.FromSeconds(interval));
            }

        end:
            ticks++;
            Output.Draw(this);
            Thread.Sleep(timeout);
        }
    }

    public void SetTimer(TimeSpan interval)
    {
        this.interval = interval.TotalSeconds;
        timer = this.interval;
    }
}
