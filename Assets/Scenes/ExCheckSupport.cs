#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace UnityAR
{
    public class ExCheckSupport : MonoBehaviour
    {
        // SerializeFieldで、アタッチ後にUnityエディターによって
        // フィールドmessageとUIのテキストボックスTextを関連付ける。
        [SerializeField]
        Text message;

        // AR Sessionの情報を得るためのフィールド
        [SerializeField]
        ARSession session;

        bool isReady;

        void ShowMessage(string text)
        {
            message.text = $"{text}\r\n";
        }

        void AddMessage(string text)
        {
            message.text += $"{text}\r\n";
        }

        void Awake()
        {
            // messageがnullの場合、アプリを終了
            if (message == null)
            {
                Application.Quit();
            }

            if (session == null)
            {
                isReady = false;
                ShowMessage("エラー：SerializeFieldの設定不備");
            }
            else
            {
                isReady = true;
                ShowMessage("ARサポート調査");
            }
        }

        IEnumerator CheckSupport()
        {
            yield return ARSession.CheckAvailability();

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                AddMessage("ARサービスのソフトウェアの更新が必要です。インストールします。");
                yield return ARSession.Install();
            }

            if (
                ARSession.state == ARSessionState.NeedsInstall
                || ARSession.state == ARSessionState.Installing
            )
            {
                AddMessage("ソフトウェアの更新に失敗、または更新を拒否しました。");
                AddMessage($"State: {ARSession.state}");
                yield break;
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                AddMessage("このデバイスはARをサポートしていません。");
                AddMessage($"State: {ARSession.state}");
                yield break;
            }

            AddMessage("このデバイスはARをサポートしています。");
            AddMessage("ARセッションの初期化。。。");
            session.enabled = true;
            const float Interval = 30f;
            var timer = Interval;

            while (
                (
                    ARSession.state == ARSessionState.Ready
                    || ARSession.state == ARSessionState.SessionInitializing
                )
                && timer > 0
            )
            {
                var waiteTime = 0.5f;
                timer -= waiteTime;
                yield return new WaitForSeconds(waiteTime);
            }

            if (timer <= 0)
            {
                AddMessage("初期化タイムオーバー！");
                AddMessage($"State: {ARSession.state}");
                yield break;
            }

            AddMessage("初期化完了！");
            AddMessage($"State: {ARSession.state}");
        }

        void OnEnable()
        {
            if (!isReady)
            {
                return;
            }
            StartCoroutine(CheckSupport());
        }
    }
}
