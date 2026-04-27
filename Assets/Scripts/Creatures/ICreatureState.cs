public interface ICreatureState
{
    void EnteredState(Creature self);
    void Tick(Creature self);
}