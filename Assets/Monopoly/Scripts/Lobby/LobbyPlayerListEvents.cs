using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public static class LobbyPlayerListEvents
{
    public static event Action PlayersChanged;

    public static void RaisePlayersChanged()
    {
        PlayersChanged?.Invoke();
    }
}

public class LobbyPlayerListRenderer : MonoBehaviour
{
    private Coroutine redrawCoroutine;

    private void OnEnable()
    {
        LobbyPlayerListEvents.PlayersChanged += QueueRedraw;
        QueueRedraw();
    }

    private void OnDisable()
    {
        LobbyPlayerListEvents.PlayersChanged -= QueueRedraw;
    }

    public void QueueRedraw()
    {
        if (!isActiveAndEnabled) return;

        if (redrawCoroutine != null)
        {
            StopCoroutine(redrawCoroutine);
        }

        redrawCoroutine = StartCoroutine(RedrawNextFrame());
    }

    private IEnumerator RedrawNextFrame()
    {
        // Redraw ederken bir frame bekliyoruz ki UI element'leri düzgün güncellensin
        yield return null;
        Redraw();
        redrawCoroutine = null;
    }

    private void Redraw()
    {
        Transform content = transform;
        MonopolyRoomPlayer[] players = FindObjectsByType<MonopolyRoomPlayer>(FindObjectsSortMode.None)
            .Where(player => player != null && player.gameObject.activeInHierarchy)
            .OrderBy(player => player.index)
            .ThenBy(player => player.netId)
            .ToArray();

        for (int i = 0; i < players.Length; i++)
        {
            MonopolyRoomPlayer player = players[i];
            player.transform.SetParent(content, false);
            player.transform.SetSiblingIndex(i);
            player.transform.localScale = Vector3.one;
            player.RefreshLobbyVisuals();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
    }
}
