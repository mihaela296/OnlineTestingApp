using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Views.Admin;

public partial class EditUserPage : ContentPage
{
    public EditUserPage(EditUserViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        System.Diagnostics.Debug.WriteLine("EditUserPage создан");
        
        // Находим кнопки и подписываемся на событие Clicked
        var saveButton = this.FindByName<Button>("SaveButton");
        if (saveButton != null)
        {
            System.Diagnostics.Debug.WriteLine("SaveButton найден");
            saveButton.Clicked += async (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine("Кнопка Сохранить нажата!");
                
                // Проверяем, что IsValid == true
                if (viewModel.IsValid)
                {
                    await viewModel.SaveCommand.ExecuteAsync(null);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Проверьте правильность заполнения полей", "OK");
                }
            };
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("SaveButton НЕ найден!");
        }
        
        var cancelButton = this.FindByName<Button>("CancelButton");
        if (cancelButton != null)
        {
            System.Diagnostics.Debug.WriteLine("CancelButton найден");
            cancelButton.Clicked += async (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine("Кнопка Отмена нажата!");
                await viewModel.CancelCommand.ExecuteAsync(null);
            };
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("CancelButton НЕ найден!");
        }
    }
}