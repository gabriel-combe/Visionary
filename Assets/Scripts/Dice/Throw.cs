using UnityEngine;

public class Throw : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;

    private Dice die;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ThrowObject();
        }

        if (die != null && die.IsImmobile)
        {
            Debug.Log(die.GetResult());
        }
    }

    public void ThrowObject()
    {
        die = Instantiate(_prefab, transform).GetComponent<Dice>();
        die.ThrowDice(transform.forward);
    }
}
