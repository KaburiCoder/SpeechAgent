using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SpeechAgent.Bases;
using static SpeechAgent.Services.ViewModelFactory;

namespace SpeechAgent.Services
{
  public interface IViewModelFactory
  {
    ReturnType<TView, TViewModel> CreateViewModel<TView, TViewModel>(Window? owner = null)
      where TView : Window, new()
      where TViewModel : BaseViewModel;
    ReturnType<TView, TViewModel> CreateViewModel<TView, TViewModel, TArgs>(
      TArgs args,
      Window? owner = null
    )
      where TView : Window, new()
      where TViewModel : BaseViewModel<TArgs>;
  }

  public class ViewModelFactory : IViewModelFactory
  {
    public class ReturnType<TView, TViewModel>
      where TView : Window
      where TViewModel : BaseViewModel
    {
      public TView View { get; set; } = default!;
      public TViewModel ViewModel { get; set; } = default!;
    }

    public ReturnType<TView, TViewModel> CreateViewModel<TView, TViewModel>(Window? owner = null)
      where TView : Window, new()
      where TViewModel : BaseViewModel
    {
      var view = new TView();
      if (owner != null)
      {
        view.Owner = owner;
      }
      var viewModel = App.Current.Services.GetRequiredService<TViewModel>();
      viewModel.SetView(view);
      viewModel.Initialize();

      view.DataContext = viewModel;
      return new ReturnType<TView, TViewModel> { View = view, ViewModel = viewModel };
    }

    public ReturnType<TView, TViewModel> CreateViewModel<TView, TViewModel, TArgs>(
      TArgs args,
      Window? owner = null
    )
      where TView : Window, new()
      where TViewModel : BaseViewModel<TArgs>
    {
      var view = new TView();
      if (owner != null)
      {
        view.Owner = owner;
      }
      var viewModel = App.Current.Services.GetRequiredService<TViewModel>();
      viewModel.SetView(view);
      viewModel.Initialize(args);

      view.DataContext = viewModel;
      return new ReturnType<TView, TViewModel> { View = view, ViewModel = viewModel };
    }
  }
}
