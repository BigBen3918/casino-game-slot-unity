mergeInto(LibraryManager.library, {
  GameController: function (msg) {
    window.dispatchReactUnityEvent(
      "GameController",
      Pointer_stringify(msg)
    );
  },
});