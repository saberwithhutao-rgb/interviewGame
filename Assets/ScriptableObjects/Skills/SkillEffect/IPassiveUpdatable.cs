public interface IPassiveUpdatable
{
    void OnUpdate(SkillContext context, PassiveEffectRuntime runtime, float deltaTime);
}