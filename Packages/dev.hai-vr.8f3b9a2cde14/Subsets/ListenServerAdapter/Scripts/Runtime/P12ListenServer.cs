using System;
using System.Collections;
using System.IO;
using System.Net;
using Basis.Scripts.Networking;
using Lavender.Systems;
using UnityEngine;

namespace Hai.Project12.ListenServer.Runtime
{
    /// WARNING: Having this class enabled in your scene will:
    /// - Start the Basis server as a separate process (BasisNetworkConsole.exe), and
    /// - Make you join that server.
    public class P12ListenServer : MonoBehaviour, IDisposable
    {
        public const int Port = 4296;
        public uint debug_pid = 0;

        private string _serverProcessPath;
        private int _port;
        private uint _pid = 0;

        private void OnEnable()
        {
            _serverProcessPath = "../Basis Server/BasisServerConsole/bin/Release/net9.0/BasisNetworkConsole.exe";
            _port = Port;

            if (File.Exists(_serverProcessPath))
            {
                // "il2cpp does not support process.start"
                // https://discord.com/channels/1239242259392757822/1344031246216335480/1344151480596430883
                _pid = StartExternalProcess.Start(_serverProcessPath, Directory.GetCurrentDirectory());
                debug_pid = _pid;

                BasisDebug.Log($"Basis server started, PID is {_pid}", BasisDebug.LogTag.Networking);

                // TODO: Linkup listen server
                // TODO: Load local client into that local server

                StartCoroutine(nameof(ConnectToLocalServer));
            }
            else
            {
                BasisDebug.LogError($"Basis server could not be found at {_serverProcessPath}.", BasisDebug.LogTag.Networking);
            }
        }

        public IEnumerator ConnectToLocalServer()
        {
            yield return new WaitForSeconds(1f);

            BasisNetworkManagement.Instance.Ip = IPAddress.Loopback.ToString();
            BasisNetworkManagement.Instance.Password = "default_password";
            BasisNetworkManagement.Instance.IsHostMode = true;
            BasisNetworkManagement.Instance.Port = Port;
            BasisNetworkManagement.Instance.Connect();
        }

        private void OnDisable()
        {
            KillServer();
        }

        public void Dispose()
        {
            KillServer();
        }

        private void KillServer()
        {
            if (_pid != 0)
            {
                try
                {
                    BasisNetworkManagement.Instance.NetworkClient.Disconnect();
                }
                catch (Exception e)
                {
                }

                // TODO: Clean shutdown
                BasisDebug.Log($"Killing server at PID {_pid}", BasisDebug.LogTag.Networking);
                try
                {
                    StartExternalProcess.KillProcess(_pid);
                    BasisDebug.Log("Killed server.", BasisDebug.LogTag.Networking);
                }
                catch (Exception e)
                {
                    BasisDebug.LogError("Failed to kill server.", BasisDebug.LogTag.Networking);
                }
                _pid = 0;
            }
        }
    }
}
