namespace Rondo.Unity {
    public static class Lifecycle {
        public delegate void Update(float deltaTime, IPresenter presenter);
        public static event Update OnUpdate;

        public static void TriggerUpdate(float deltaTime, IPresenter presenter) {
            OnUpdate?.Invoke(deltaTime, presenter);
        }
    }
}