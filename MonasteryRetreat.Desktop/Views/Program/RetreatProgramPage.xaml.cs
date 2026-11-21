using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MonasteryRetreat.Application.Program;

namespace MonasteryRetreat.Desktop.Views.Program;

public partial class RetreatProgramPage : UserControl
{
    private readonly IRetreatProgramService _service;
    private IReadOnlyList<RetreatProgramRow> _rows = Array.Empty<RetreatProgramRow>();
    private long? _editingId;
    private bool _loading;

    public RetreatProgramPage(IRetreatProgramService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        InitializeComponent();
        Loaded += async (_, _) => await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_loading) return;
        _loading = true;
        try
        {
            _rows = await _service.ListAsync();
            ProgramGrid.ItemsSource = _rows;
            EmptyText.Visibility = _rows.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل البرنامج:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { _loading = false; }
    }

    private void ResetEditor()
    {
        _editingId = null;
        EditorTitle.Text = "إضافة نشاط";
        TitleBox.Clear();
        DescriptionBox.Clear();
        StartBox.Text = "07:00";
        EndBox.Clear();
        OrderBox.Text = (_rows.Count == 0 ? 10 :
            _rows.Max(x => x.DisplayOrder) + 10).ToString(CultureInfo.InvariantCulture);
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not RetreatProgramRow row)
            return;
        _editingId = row.Id;
        EditorTitle.Text = "تعديل النشاط";
        TitleBox.Text = row.TitleAr;
        DescriptionBox.Text = row.DescriptionAr ?? string.Empty;
        StartBox.Text = row.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        EndBox.Text = row.EndTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "";
        OrderBox.Text = row.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        TitleBox.Focus();
    }

    private void CancelEdit_Click(object sender, RoutedEventArgs e) => ResetEditor();

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TimeOnly.TryParseExact(StartBox.Text.Trim(), "HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
        {
            MessageBox.Show("أدخل وقت البداية بصيغة 24 ساعة HH:mm.", "بيانات غير صحيحة");
            StartBox.Focus();
            return;
        }

        TimeOnly? end = null;
        if (!string.IsNullOrWhiteSpace(EndBox.Text))
        {
            if (!TimeOnly.TryParseExact(EndBox.Text.Trim(), "HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                MessageBox.Show("أدخل وقت النهاية بصيغة HH:mm أو اتركه فارغًا.",
                    "بيانات غير صحيحة");
                EndBox.Focus();
                return;
            }
            end = parsed;
        }

        if (!int.TryParse(OrderBox.Text.Trim(), NumberStyles.None,
                CultureInfo.InvariantCulture, out int order) || order < 0)
        {
            MessageBox.Show("ترتيب العرض يجب أن يكون عددًا صحيحًا موجبًا أو صفرًا.",
                "بيانات غير صحيحة");
            OrderBox.Focus();
            return;
        }

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.SaveAsync(new SaveRetreatProgramItem(
                _editingId, TitleBox.Text, DescriptionBox.Text, start, end, order));
            await RefreshAsync();
            ResetEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر حفظ النشاط:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { ContentRoot.IsEnabled = true; }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not RetreatProgramRow row)
            return;
        if (MessageBox.Show($"حذف النشاط «{row.TitleAr}» من البرنامج؟",
                "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question)
            != MessageBoxResult.Yes) return;

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.DeleteAsync(row.Id);
            await RefreshAsync();
            if (_editingId == row.Id) ResetEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر حذف النشاط:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { ContentRoot.IsEnabled = true; }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
        => await RefreshAsync();

    private async void Sample_Click(object sender, RoutedEventArgs e)
    {
        if (_rows.Count > 0)
        {
            MessageBox.Show("يوجد برنامج محفوظ بالفعل. لا يمكن تحميل النموذج فوق البيانات الحالية.",
                "برنامج الخلوة", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show(
                "سيتم تحميل نموذج قابل للتعديل مستوحى من الصورة، ومواعيده تحتاج إلى مراجعتك قبل الطباعة. هل تريد المتابعة؟",
                "تحميل النموذج", MessageBoxButton.YesNo, MessageBoxImage.Question)
            != MessageBoxResult.Yes) return;

        ContentRoot.IsEnabled = false;
        try
        {
            if (!await _service.LoadSampleAsync())
            {
                MessageBox.Show("يوجد برنامج محفوظ أو مؤرشف بالفعل.", "برنامج الخلوة");
                return;
            }
            await RefreshAsync();
            ResetEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل النموذج:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { ContentRoot.IsEnabled = true; }
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (_rows.Count == 0)
        {
            MessageBox.Show("أضف نشاطًا واحدًا على الأقل قبل الطباعة.", "برنامج الخلوة");
            return;
        }

        try
        {
            // The Windows printer dialog cannot preview arbitrary WPF
            // PrintDocument jobs. Show an actual paginated preview first.
            var preview = new Window
            {
                Title = "معاينة طباعة برنامج الخلوة",
                Owner = Window.GetWindow(this),
                Width = 960,
                Height = 780,
                MinWidth = 650,
                MinHeight = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                FlowDirection = FlowDirection.RightToLeft,
                Background = Brushes.White
            };

            var layout = new DockPanel();
            var toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(12)
            };
            DockPanel.SetDock(toolbar, Dock.Top);

            var printButton = new Button
            {
                Content = "طباعة / حفظ PDF",
                MinWidth = 160,
                Height = 38,
                Margin = new Thickness(0, 0, 10, 0),
                Background = (Brush)System.Windows.Application.Current.Resources["Brown"],
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            printButton.Click += (_, _) => PrintDocument();

            var closeButton = new Button
            {
                Content = "إغلاق المعاينة",
                MinWidth = 120,
                Height = 38
            };
            closeButton.Click += (_, _) => preview.Close();
            toolbar.Children.Add(printButton);
            toolbar.Children.Add(closeButton);

            var reader = new FlowDocumentReader
            {
                Document = BuildPrintDocument(793.7, 1122.5), // A4 at 96 DPI
                ViewingMode = FlowDocumentReaderViewingMode.Page,
                IsPageViewEnabled = true,
                IsScrollViewEnabled = false,
                IsTwoPageViewEnabled = false,
                IsFindEnabled = false,
                IsPrintEnabled = false,
                Zoom = 85
            };

            layout.Children.Add(toolbar);
            layout.Children.Add(reader);
            preview.Content = layout;
            preview.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر عرض معاينة الطباعة:\\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PrintDocument()
    {
        try
        {
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true) return;

            var document = BuildPrintDocument(
                dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            paginator.PageSize = new Size(
                dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
            dialog.PrintDocument(paginator, "برنامج الخلوة اليومي");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذرت الطباعة:\\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private FlowDocument BuildPrintDocument(double pageWidth, double pageHeight)
    {
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            PageWidth = pageWidth,
            PageHeight = pageHeight,
            PagePadding = new Thickness(35, 35, 35, 35),
            ColumnWidth = double.PositiveInfinity,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 15,
            Foreground = Brushes.Black
        };
            document.Blocks.Add(new Paragraph(new Run("بيت دير السيدة العذراء السريان"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            document.Blocks.Add(new Paragraph(new Run("جدول نظام اليوم بالخلوة"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1.5, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(3, GridUnitType.Star) });
            var group = new TableRowGroup();
            table.RowGroups.Add(group);

            var header = new TableRow { Background = new SolidColorBrush(Color.FromRgb(232, 240, 244)) };
            header.Cells.Add(PrintCell("الوقت", true));
            header.Cells.Add(PrintCell("النشاط", true));
            header.Cells.Add(PrintCell("التفاصيل / الاستثناءات", true));
            group.Rows.Add(header);

            foreach (var row in _rows)
            {
                var tr = new TableRow();
                tr.Cells.Add(PrintCell(row.TimeText));
                tr.Cells.Add(PrintCell(row.TitleAr));
                tr.Cells.Add(PrintCell(row.DescriptionAr ?? "—"));
                group.Rows.Add(tr);
            }

            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run("يرجى الالتزام بمواعيد برنامج الخلوة."))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 18, 0, 0)
            });

        return document;
    }

    private static TableCell PrintCell(string text, bool bold = false)
    {
        return new TableCell(new Paragraph(new Run(text))
        {
            Margin = new Thickness(0),
            TextAlignment = TextAlignment.Right,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
        })
        {
            Padding = new Thickness(8, 9, 8, 9),
            BorderBrush = new SolidColorBrush(Color.FromRgb(216, 208, 196)),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
    }
}
