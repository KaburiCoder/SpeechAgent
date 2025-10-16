using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace SpeechAgent.Bases
{
  public abstract class BaseViewModel : ObservableObject
  {
    protected Window View = default!;
    public void SetView(Window view)
    {
      View = view;
    }

    public virtual void Initialize() { }
  }

  public abstract class BaseViewModel<TArgs> : BaseViewModel
  {
    public virtual void Initialize(TArgs args) { }
  }

}
