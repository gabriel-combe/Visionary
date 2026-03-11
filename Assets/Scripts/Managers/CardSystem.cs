using System;
using System.Collections.Generic;
using UnityEngine;

public class CardSystem : SingletonMono<CardSystem>
{
    [Serializable]
    public class CardAssociation
    {
        public Card card1;
        public Card card2;
        public Card cardEnding;
    }

    [SerializeField] private List<CardAssociation> _cardAssociations = new();

    private List<Card> _cards = new();
    public IReadOnlyList<Card> Cards => _cards;

    public Card GetCard(int slot) => slot < _cards.Count ? _cards[slot] : null;
    public Card Card1 => GetCard(0);
    public Card Card2 => GetCard(1);
    public Card EndingCard => GetCard(2);

    public bool HasCard(Card card) => _cards.Exists(c => c == card);

    // Call via EventBus after a DialogOutcome GiveCard
    public void GrantCard(Card card)
    {
        _cards.Add(card);
        EventBus.Emit(new CardUpdatedEvent(_cards));
    }

    public Card FindEnding()
    {
        var c1 = Card1;
        var c2 = Card2;
        foreach (var a in _cardAssociations)
            if ((a.card1 == c1 && a.card2 == c2) || (a.card1 == c2 && a.card2 == c1))
                return a.cardEnding;

        Debug.Log("[CardSystem] No matching ending, using default.");
        return _cardAssociations[^1].cardEnding;
    }

    public void ResetCards() => _cards.Clear();
}
