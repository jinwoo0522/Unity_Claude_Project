// 풀에 스스로 반납을 요청할 때 쓰는 비제네릭 핸들 — 풀링 대상이 자기 타입을 몰라도 반납 가능
public interface IPoolReturner
{
    void Return(object obj);
}
