using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeedLimit = 50;

    private Canvas _canvas;
    private Image _imageComponent;
    private Vector2 _offset;
    private Vector3 _targetPosition;

    private bool _isDragging = false;
    private bool _hasBeenDragged = false;

    [Header("Rotation")]
    [SerializeField] private float rotationAmount = 0.5f;
    [SerializeField] private float rotationSpeed = 5.0f;
    private Vector3 movementDelta = Vector3.zero;
    private Vector3 rotationDelta = Vector3.zero;

    public float lerpSpeed = 0.1f; // Adjust the lerp speed as needed
    [SerializeField] private Ease _ease;

    private void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        _imageComponent = GetComponent<Image>();
    }

    private void Update()
    {
        if (_isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, lerpSpeed);
            FollowRotation();
        }
    }

    private void FollowRotation()
    {
        Vector3 movement = (transform.position - _targetPosition);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = movementDelta * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _offset = mousePosition - (Vector2)transform.position;
        _offset = eventData.position - (Vector2)transform.position;

        _isDragging = true;
        _canvas.GetComponent<GraphicRaycaster>().enabled = false;
        _imageComponent.raycastTarget = false;

        _hasBeenDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Calculate the target position during dragging
        _targetPosition = eventData.position - _offset;
        ClampInsideBounds();
    }

    private void ClampInsideBounds()
    {
        // Clamp the target position within screen bounds
        Vector3[] canvasCorners = new Vector3[4];
        _canvas.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);

        float canvasLeft = canvasCorners[0].x;
        float canvasRight = canvasCorners[2].x;
        float canvasBottom = canvasCorners[0].y;
        float canvasTop = canvasCorners[2].y;

        float halfWidth = GetComponent<RectTransform>().rect.width / 2;
        float halfHeight = GetComponent<RectTransform>().rect.height / 2;

        _targetPosition.x = Mathf.Clamp(_targetPosition.x, canvasLeft + halfWidth, canvasRight - halfWidth);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, canvasBottom + halfHeight, canvasTop - halfHeight);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        _canvas.GetComponent<GraphicRaycaster>().enabled = true;
        _imageComponent.raycastTarget = true;
        _hasBeenDragged = false;

        // Rotate back to 0 degrees on z-axis using DOTween
        transform.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(1.1f, 0.1f).SetEase(_ease).OnComplete(() =>
        {
            transform.DOScale(1f, 0.1f).SetEase(_ease);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}
