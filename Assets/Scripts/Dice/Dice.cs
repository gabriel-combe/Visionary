using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class Dice : MonoBehaviour
{
    [SerializeField] private float _throwForce = 800f;
    [SerializeField] private float _torqueMultiplier = 0.5f;
    [SerializeField] private float _immobileThreshold = 0.05f;

    private Rigidbody _rigidbody;

    public bool IsImmobile => _rigidbody != null && _rigidbody.angularVelocity.magnitude < _immobileThreshold;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void ThrowDice(Vector3 dir)
    {
        if (_rigidbody == null)
        {
            Debug.LogError("Rigidbody component is missing on the dice.");
            return;
        }

        _rigidbody.AddForce(dir.normalized * _throwForce, ForceMode.Impulse);
        var torque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * _torqueMultiplier;
        _rigidbody.AddTorque(torque, ForceMode.Impulse);
    }

    public int GetResult()
    {
        if (!IsImmobile) return 0;

        float maxY = float.MinValue;
        GameObject selectedChild = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild (i);
            if (child.position.y > maxY)
            {
                maxY = child.position.y;
                selectedChild = child.gameObject;
            }
        }

        if (selectedChild != null && int.TryParse(selectedChild.name, out int value)) return value;
        return 0;
    }
}
