using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using System.Timers;


namespace MiFirstService
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        ConfigJson conf = new ConfigJson(); //вызываем для создания файла настроек если его нет 
        Logging log = new Logging();
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanShutdown = true;
        }
        
        protected override void OnStart(string[] args)
        {


            //log.log(conf.GetConfigFilePath());
            //Console.ReadLine();
            if (!Directory.Exists(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log")))
            {
                Directory.CreateDirectory(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log"));
            }
            Directory.GetFiles(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log"))
         .Select(f => new FileInfo(f))
         .Where(f => f.LastAccessTime < DateTime.Now.AddDays(-30))
         .ToList()
         .ForEach(f => f.Delete());
            log.log("==============================Service Started======================");
            timer.Elapsed += new ElapsedEventHandler(Myaction);
            timer.Interval = 60000; //number in milisecinds  
            timer.Enabled = true;


        }
        public void Myaction(object source, ElapsedEventArgs e)
        {

            //создаём экземпляр настроек 

                Settings settings = (Settings)conf.GetSettings();
                KeyRequst keyRequst = new KeyRequst();
                if (settings.SKey == null) // the are no Skey
                {
                    var skey = keyRequst.GetSekretKey();
                    if (skey == null)
                    {
                        log.log($"Server not unswer, check settins in the {conf.GetConfigFilePath()}");
                        log.log($"Is the Central server on {settings.ServerAdress}:{settings.Port}");
                    }
                    if (skey != null)
                    {
                        if (skey.Item1 == 200)
                        {
                            settings.SKey = skey.Item2;
                            conf.SetSettings(settings);
                        }
                        if (skey.Item1 == 202)// We have to wait for a manual verification
                        {
                            log.log("Need to manual approve from the Admin Console  ");
                            while (skey.Item1 != 200)
                            {
                                skey = keyRequst.GetSekretKey();
                                Thread.Sleep(5 * 1000);
                            }
                            settings.SKey = skey.Item2;
                            conf.SetSettings(settings);
                        }
                        if (skey.Item1 == 503)// We have to wait for a manual verification
                        {
                            log.log("Server not ready need to wait ");
                            while (skey.Item1 != 200)
                            {
                                skey = keyRequst.GetSekretKey();
                                Thread.Sleep(5 * 1000);
                            }
                            settings.SKey = skey.Item2;
                            conf.SetSettings(settings);
                        }
                        if (skey.Item1 == 409)
                        {
                            log.log("another custom_service alredy registred, need to unregistred fron the Admin console ");
                        }
                        if (skey.Item1 == 403)
                        {
                            log.log("Forbidden");
                        }

                        log.log($"Server not unswer, check settins in the {conf.GetConfigFilePath()}");
                        log.log($"Is the Central server on {settings.ServerAdress}:{settings.Port}");
                    }

                }

                if (settings.Token == null)
                {
                    var token = keyRequst.GetToken();
                    if (token == null)
                    {
                        log.log($"Server not unswer, check settins in the {conf.GetConfigFilePath()}");
                        log.log($"Is the Central server on {settings.ServerAdress}:{settings.Port}");
                    }
                    if (token != null)
                    {
                        if (token.Item1 == 200)
                        {
                            settings.Token = token.Item2;
                            conf.SetSettings(settings);
                            log.log("Token received " + token.Item2);
                        }
                        if (token.Item1 == 503)// server not ready 
                        {
                            log.log("Server not ready need to wait ");
                            while (token.Item1 != 200)
                            {
                                token = keyRequst.GetToken();
                                Thread.Sleep(5 * 1000);
                            }
                            settings.SKey = token.Item2;
                            conf.SetSettings(settings);
                        }


                    }
                }

                int keyCheck = keyRequst.CheckToken();

                if (keyCheck != 0)
                {
                    if (keyCheck == 403) //probably wrong token try to take new one
                    {
                        log.log("The remote server returned an error: (403) Forbidden");
                        settings.Token = null;
                        conf.SetSettings(settings);
                        var token = keyRequst.GetToken();
                        if (token == null)
                        {
                            log.log($"The server does not respond, check settins in the {conf.GetConfigFilePath()}");
                            log.log($"Is the Central server on {settings.ServerAdress}:{settings.Port}");
                        }
                        if (token.Item1 == 200 & token != null)
                        {
                            settings.Token = token.Item2;
                            conf.SetSettings(settings);
                            log.log("Token received " + token.Item2);
                        }
                        if (token.Item1 == 503 & token != null)// server not ready 
                        {
                            log.log("Server not ready need to wait ");
                            while (token.Item1 != 200 & token != null)
                            {
                                token = keyRequst.GetToken();
                                Thread.Sleep(5 * 1000);
                            }
                            settings.SKey = token.Item2;
                            conf.SetSettings(settings);
                        }
                        if (token.Item1 == 401) //не получилось с текущим ключем получить токен
                        {
                            settings.SKey = null;
                            if (settings.SKey == null) // the are no Skey
                            {
                                var skey = keyRequst.GetSekretKey();
                                if (skey.Item1 == 200 & skey != null)
                                {
                                    settings.SKey = skey.Item2;
                                    conf.SetSettings(settings);
                                }
                                if (skey.Item1 == 202 & skey != null)// We have to wait for a manual verification
                                {
                                    log.log("Need to manual approve from the Admin Console  ");
                                    while (skey.Item1 != 200 & skey != null)
                                    {
                                        skey = keyRequst.GetSekretKey();
                                        Thread.Sleep(5 * 1000);
                                    }
                                    settings.SKey = skey.Item2;
                                    conf.SetSettings(settings);
                                }
                                if (skey.Item1 == 503)// We have to wait for a manual verification
                                {
                                    log.log("Server not ready need to wait ");
                                    while (skey.Item1 != 200 & skey != null)
                                    {
                                        skey = keyRequst.GetSekretKey();
                                        Thread.Sleep(5 * 1000);
                                    }
                                    settings.SKey = skey.Item2;
                                    conf.SetSettings(settings);
                                }
                                if (skey.Item1 == 409 & skey != null)
                                {
                                    log.log("another custom_service alredy registred, need to unregistred fron the Admin console ");
                                }
                                if (skey.Item1 == 403 & skey != null)
                                {
                                    log.log("Forbidden");
                                }
                                if (skey == null)
                                {
                                    log.log($"Server not unswer, check settins in the {conf.GetConfigFilePath()}");
                                    log.log($"Is the Central server on {settings.ServerAdress}:{settings.Port}");
                                }
                                //=====================================================//
                                //опять запрашиваем токен
                                conf.SetSettings(settings);
                                token = keyRequst.GetToken();
                                if (token == null)
                                {
                                    log.log($"Server not unswer, check settins in the {conf.GetConfigFilePath()}");
                                    log.log($"Is the Central server on {settings.ServerAdress}:{settings.Port}");
                                }
                                if (token.Item1 == 200 & token != null)
                                {
                                    settings.Token = token.Item2;
                                    conf.SetSettings(settings);
                                    log.log("Token received " + token.Item2);
                                }
                                if (token.Item1 == 503 & token != null)// server not ready 
                                {
                                    log.log("Server not ready need to wait ");
                                    while (token.Item1 != 200 & token != null)
                                    {
                                        token = keyRequst.GetToken();
                                        Thread.Sleep(5 * 1000);
                                    }
                                    settings.SKey = token.Item2;
                                    conf.SetSettings(settings);
                                }
                                //========================================================//
                            }

                        }

                    }
                }


               

            
        }

        protected override void OnStop()
        {
            log.log("==============================Service Ended======================");
        }

    }
    public class Logging
    {
        public void log(string log)
        {
            string pathToLogFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log", "custom_service_" + DateTime.Now.ToString("dd.MM.yyyy") + ".log");
            string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.FFF");
            File.AppendAllText(pathToLogFile, $"{date} : {log} \n");

        }
    }
}
