namespace UI.Components
{
    public class LetterBooster : BoosterUI
    {
        public override void ActivateBooster()
        {
            IsActive = true;
            // TODO: - - -
        }
        
        private void Finish()
        {
            
            IsActive = false;
        }
    }
}