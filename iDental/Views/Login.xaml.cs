﻿using iDental.Class;
using iDental.DatabaseAccess.DatabaseObject;
using iDental.DatabaseAccess.QueryEntities;
using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace iDental.Views
{
    /// <summary>
    /// Login.xaml 的互動邏輯
    /// </summary>
    public partial class Login : Window
    {
        /// <summary>
        /// 載入的機構資訊
        /// </summary>
        public Agencys Agencys { get; private set; }
        /// <summary>
        /// 載入的病患資訊
        /// </summary>
        public Patients Patients { get; private set; }

        private bool MessageBoxStatus = false;
        private string MessageBoxTips = string.Empty;
        private bool ReturnDialogResult = false;

        public Login()
        {
            InitializeComponent();
        }

        private void Window_Login_ContentRendered(object sender, EventArgs e)
        {
            //生命週期在 Window.Loaded 後觸發
            //畫面Rendered完顯示
            Loading();
        }

        /// <summary>
        /// 開始載入判斷
        /// </summary>
        private void Loading()
        {
            try
            {
                TextBlockStatus.Dispatcher.Invoke(() =>
                {
                    TextBlockStatus.Text = "伺服器位置確認中";
                }, DispatcherPriority.Render);

                //判斷app.config
                if (!ConfigManage.ReadSetting("Server"))//尚未設置
                {
                    AnswerDialogOne answerDialogOne = new AnswerDialogOne("請輸入伺服器位置:", "IP");
                    if (answerDialogOne.ShowDialog() == true)
                    {
                        //寫入config Server 欄位
                        string serverIP = answerDialogOne.Answer;
                        ConfigManage.AddUpdateAppCongig("Server", serverIP);
                    }
                }

                TextBlockStatus.Dispatcher.Invoke(() =>
                {
                    TextBlockStatus.Text = "嘗試連接伺服器...";
                }, DispatcherPriority.Render);

                //連接Server connection
                if (new ConnectionString().CheckConnection())
                {
                    TextBlockStatus.Dispatcher.Invoke(() =>
                    {
                        TextBlockStatus.Text = "取得本機資訊";
                    }, DispatcherPriority.Render);

                    //取得本機訊息
                    //HostName、IP
                    string hostName = Dns.GetHostName();
                    string localIP = string.Empty;
                    IPHostEntry ipHostEntry = Dns.GetHostEntry(hostName);
                    foreach (IPAddress ip in ipHostEntry.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            localIP = ip.ToString();
                        }
                    }

                    TableAgencys tableAgencys = new TableAgencys();
                    //取得已經驗證的機構
                    Agencys agencys = tableAgencys.QueryVerifyAgencys();
                    if (agencys != null)
                    {
                        Agencys = agencys;

                        //開始判斷本機與伺服器關係
                        TableClients tableClients = new TableClients();
                        Clients clients = tableClients.QueryClient(hostName);
                        if (clients != null)
                        {
                            if (clients.Agency_VerificationCode.Equals(agencys.Agency_VerificationCode))
                            {
                                if (clients.Client_IsVerify)
                                {
                                    TextBlockStatus.Dispatcher.Invoke(() =>
                                    {
                                        TextBlockStatus.Text = "本機已認證";
                                    }, DispatcherPriority.Render);

                                    //本機與伺服器認證通過
                                    //判斷軟體使用期限
                                    CheckServerTrialPeriod(agencys);
                                }
                                else
                                {
                                    MessageBoxTips = "此台電腦已經被停用，請聯絡資訊廠商...";
                                }
                            }
                            else
                            {
                                MessageBoxTips = "此台電腦與伺服器的驗證碼不符，請聯絡資訊廠商";
                            }
                        }
                        else
                        {
                            //第一次使用，輸入驗證碼
                            AnswerDialogOne answerDialogOne = new AnswerDialogOne("此台電腦為第一次登入，請輸入產品驗證碼:", "Verify");
                            if (answerDialogOne.ShowDialog() == true)
                            {
                                string verificationCodeClient = string.Empty;
                                verificationCodeClient = answerDialogOne.Answer;
                                if (verificationCodeClient.Equals(agencys.Agency_VerificationCode))
                                {
                                    TextBlockStatus.Dispatcher.Invoke(() =>
                                    {
                                        TextBlockStatus.Text = "本機與伺服器驗證成功";
                                    }, DispatcherPriority.Render);

                                    Clients insertClients = new Clients()
                                    {
                                        Client_HostName = hostName,
                                        Client_IP = localIP,
                                        Client_IsVerify = true,
                                        Agency_VerificationCode = verificationCodeClient
                                    };
                                    tableClients.InsertClient(insertClients);

                                    //本機與伺服器認證通過
                                    //判斷軟體使用期限
                                    CheckServerTrialPeriod(agencys);
                                }
                                else
                                {
                                    MessageBoxTips = "與伺服器的驗證碼不符，請聯絡資訊廠商";
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBoxTips = "伺服器尚未被驗證，無法使用，請聯絡資訊廠商";
                    }

                    //寫入ConnectingLog資訊
                    TableConnectingLogs tableConnectingLogs = new TableConnectingLogs();
                    ConnectingLogs connectingLogs = new ConnectingLogs()
                    {
                        ConnectingLog_HostName = hostName,
                        ConnectingLog_IP = localIP,
                        ConnectingLog_Error = MessageBoxTips,
                        ConnectingLog_IsPermit = ReturnDialogResult
                    };
                    tableConnectingLogs.InsertConnectingLog(connectingLogs);
                }
                else
                {
                    MessageBoxTips = "伺服器連接失敗";
                    ConfigManage.AddUpdateAppCongig("Server", "");
                }

                //show MessageBox
                if (ReturnDialogResult)
                {
                    TextBlockStatus.Dispatcher.Invoke(() =>
                    {
                        TextBlockStatus.Text = "成功登入，歡迎使用iDental";
                    }, DispatcherPriority.Render);

                    if (MessageBoxStatus)//還在試用期內可以使用
                    {
                        MessageBox.Show(MessageBoxTips, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    TextBlockStatus.Dispatcher.Invoke(() =>
                    {
                        TextBlockStatus.Text = "登入失敗，原因:" + MessageBoxTips;
                    }, DispatcherPriority.Render);

                    MessageBox.Show(MessageBoxTips, "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                Thread.Sleep(2000);
                //回傳結果
                DialogResult = ReturnDialogResult;
            }
            catch (Exception ex)
            {
                ErrorLog.ErrorMessageOutput(ex.ToString());
                DialogResult = false;
            }
            Close();
        }

        /// <summary>
        /// 本機資訊與伺服器資訊都沒問題後的版本判斷
        /// </summary>
        /// <param name="agencys">Agencys</param>
        private void CheckServerTrialPeriod(Agencys agencys)
        {
            //開始判斷伺服器認證碼的使用權限
            TextBlockStatus.Dispatcher.Invoke(() =>
            {
                TextBlockStatus.Text = "取得伺服器認證資訊";
            }, DispatcherPriority.Render);

            if (agencys.Agency_IsVerify)
            {
                if (agencys.Agency_IsTry)
                {
                    if (agencys.Agency_TrialPeriod < DateTime.Now.Date)
                    {
                        MessageBoxTips = "試用期限已到，請聯絡資訊廠商";
                    }
                    else
                    {
                        MessageBoxStatus = true;

                        MessageBoxTips = "此為試用版本，試用日期至" + ((DateTime)agencys.Agency_TrialPeriod).ToShortDateString();

                        TextBlockStatus.Dispatcher.Invoke(() =>
                        {
                            TextBlockStatus.Text = "病患資訊確認中...";
                        }, DispatcherPriority.Render);

                        //載入病患
                        LoadPatient();

                        ReturnDialogResult = true;
                    }
                }
                else
                {
                    TextBlockStatus.Dispatcher.Invoke(() =>
                    {
                        TextBlockStatus.Text = "病患資訊確認中...";
                    }, DispatcherPriority.Render);

                    //載入病患
                    LoadPatient();

                    ReturnDialogResult = true;
                }
            }
            else
            {
                MessageBoxTips = "此驗證碼已停用，如欲使用請聯絡資訊廠商";
            }
        }

        /// <summary>
        /// 新增新病患
        /// </summary>
        private void LoadPatient()
        {
            //Data from AppStartup
            if (((Application.Current as App).Patients).Patient_ID != null)
            {
                Patients patients = (Application.Current as App).Patients;
                TablePatients tablePatients = new TablePatients();
                Patients = tablePatients.QueryNewOldPatient(patients);
            }
        }
    }
}
