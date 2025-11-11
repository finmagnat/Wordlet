using Game.Logic;

namespace Game.PvP
{
    /*
     * PvP через Firebase (реальное время):
        каждый игрок пишет свой ход в ветку rooms/{roomId}/turns.
        второй слушает изменения и применяет ход.
     */
    public class PvPGameController : IState
    {
        
    }
}