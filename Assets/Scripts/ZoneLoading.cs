using UnityEngine;

public class ZoneLoading : MonoBehaviour
{
    [SerializeField]
    private Pallete PalletePrefab;


    public void SpawnPallete()
    {
        var pallete = Instantiate(PalletePrefab, transform.position, Quaternion.identity);
        pallete.Anim(PalleteAnimType.LoadingZone);
    }
}
