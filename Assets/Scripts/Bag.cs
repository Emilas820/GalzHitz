using UnityEngine;

public class Bag : MonoBehaviour
{
    private float damage;

    // 생성되는 순간 호출될 함수
    public void Setup(BagData data) {
        this.damage = data.damage; // 데이터 파일에 있는 숫자를 가져옴
        Debug.Log(data.name + " 가방 생성! 데미지는: " + this.damage);
    }

}
