using UnityEngine;

public class ZoneLoading : MonoBehaviour
{
    [SerializeField]
    private Pallete PalletePrefab;


    public void SpawnPallete()
    {
        Instantiate(PalletePrefab, transform.position, Quaternion.identity);
    }
}
