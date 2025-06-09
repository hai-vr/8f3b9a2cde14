using System;
using System.Collections;
using System.IO;
using System.Net;
using Basis.Scripts.Networking;
using Hai.Project12.HaiSystems.Supporting;
using UnityEngine;
// FIXME: Unavailable outside those builds. Can't be bothered figuring this out properly for now.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using Lavender.Systems;
#endif

namespace Hai.Project12.ListenServer.Runtime
{
    /// WARNING: Having this class enabled in your scene will:
    /// - Start the Basis server as a separate process (BasisNetworkConsole.exe), and
    /// - Make you join that server.
    public class P12ListenServer : MonoBehaviour
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

// FIXME: Unavailable outside those builds. Can't be bothered figuring this out properly for now.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (File.Exists(_serverProcessPath))
            {
                // "il2cpp does not support process.start"
                // https://discord.com/channels/1239242259392757822/1344031246216335480/1344151480596430883
                _pid = StartExternalProcess.Start(_serverProcessPath, Directory.GetCurrentDirectory());
                debug_pid = _pid;

                H12Debug.Log($"Basis server started, PID is {_pid}", H12Debug.LogTag.ListenNetworking);

                // TODO: Linkup listen server
                // TODO: Load local client into that local server

                StartCoroutine(nameof(ConnectToLocalServer));
            }
            else
            {
                H12Debug.LogError($"Basis server could not be found at {_serverProcessPath}.", H12Debug.LogTag.ListenNetworking);
            }
#endif
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

        private void OnDestroy()
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
                H12Debug.Log($"Killing server at PID {_pid}", H12Debug.LogTag.ListenNetworking);
                try
                {
                    StartExternalProcess.KillProcess(_pid);
                    H12Debug.Log("Killed server.", H12Debug.LogTag.ListenNetworking);
                }
                catch (Exception e)
                {
                    H12Debug.LogError("Failed to kill server.", H12Debug.LogTag.ListenNetworking);
                }
                _pid = 0;
            }
        }
    }
}
