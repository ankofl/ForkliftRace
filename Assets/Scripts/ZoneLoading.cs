using UnityEngine;

public class ZoneLoading : MonoBehaviour
{
    [SerializeField]
    private Pallete PalletePrefab;


	public void SpawnPallete(ref Pallete pallete)
    {
        if(pallete != null)
        {
            Destroy(pallete.gameObject);
		}

        pallete = Instantiate(PalletePrefab, transform.position, Quaternion.identity);
        pallete.Anim(PalleteAnimType.LoadingZone);
    }
}
