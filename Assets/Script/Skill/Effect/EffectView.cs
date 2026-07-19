using UnityEngine;

// 풀링되는 이펙트 뷰 — 스킬 수명과 독립적으로 재생되고, 재생이 끝나면 스스로 풀에 반납한다.
// 순수 표현 계층이므로 NetworkObject가 아니라 각 클라이언트 로컬로 동작한다.
public class EffectView : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _particle;
    private bool _isReturning;   // 재생 종료 콜백이 중복 반납하는 것을 방지

    public IPoolReturner Handler { get; set; }

    // 풀에서 꺼낼 때 — 활성화 및 반납 플래그 초기화
    public void Active()
    {
        _isReturning = false;
        gameObject.SetActive(true);
    }

    // 지정 위치/회전에서 재생 (SkillEffector가 위치를 지정한 뒤 호출)
    public void Play(Vector3 vPosition, Quaternion qRotation)
    {
        transform.SetPositionAndRotation(vPosition, qRotation);
        _particle.Play();
    }

    // Effector가 looping 이펙트를 명시적으로 정지 — StopEmitting 후 종료 콜백으로 스스로 반납
    public void Stop() => _particle.Stop();

    // 풀에 반납될 때 — 파티클 정지 및 비활성화
    public void Release()
    {
        _particle.Stop();
        gameObject.SetActive(false);
    }

    public void Destroy() => GameObject.Destroy(gameObject);

    // 파티클 재생 종료 콜백(Stop Action = Callback) — 스스로 풀에 반납
    private void OnParticleSystemStopped()
    {
        if (_isReturning) return;
        _isReturning = true;
        Handler.Return(this);
    }
}
