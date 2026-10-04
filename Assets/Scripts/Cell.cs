using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    public int x;
    public int y;

    public bool isMine;

    public int adjacentMines;

    public bool isRevealed;

    public bool isFlagged;

    public void SetIcon(Sprite sprite)
    {
        iconRenderer.sprite = sprite;
    }

    private void Awake()
    {
        backgroundRenderer = GetComponent<SpriteRenderer>();
    }
    public void Reveal()
    {
        if (isRevealed) return;

        isRevealed = true;
        backgroundRenderer.color = Color.gray;
        if (isMine)
        {
            iconRenderer.sprite = GameManager.Instance.GetExplosionSprite();
            isMine = false;
        }
        else iconRenderer.sprite = GameManager.Instance.GetNumberSprite(adjacentMines);
    }

    public bool ToggleFlag()
    {
        isFlagged = !isFlagged;

        if(isFlagged) iconRenderer.sprite = GameManager.Instance.GetFlagSprite();
        else iconRenderer.sprite = null;
        return isFlagged;
    }

    public void Mine()
    {
        if(isMine) iconRenderer.sprite = GameManager.Instance.GetMineSprite();
        else if (isFlagged) iconRenderer.sprite = GameManager.Instance.GetNotmineSprite();
    }
}
