using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Xml;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace SaveGameApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region Default values dynamicButtonGrid
        //Grid name
        private const string dynamicButtonGrid = "DynamicButtonGrid";

        //Button Data
        private const float buttonHeight = 20.0f;
        private const float buttonWidth = 70.0f;
        private const string defaultButtonText = "Browse";
        private const string buttonNamePrefix = "Button_";

        //Textbox Data
        private const float textBoxHeight = 20.0f;
        private const float textBoxWidth = 500.0f;
        #endregion

        #region Save/Load Settings
        private const string xmlRoot = "Root";
        private const string xmlButtons = "Buttons";
        private const string xmlFileName = "Saves/Data.xml";
        private const string templateXmlFileName = "Template/xmlTemplate.xml";
        private string xmlPath;

        private XmlDocument xmlDoc;
        #endregion

        int i = 0;

        public MainWindow() {
            InitializeComponent();
            RunSetup();
        }

        private void RunSetup() {
            xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFileName);

            if (!System.IO.File.Exists(xmlPath)) {
                string templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templateXmlFileName);
                XmlDocument saveXml = new XmlDocument();
                saveXml.Load(templatePath);
                saveXml.Save(xmlPath);

                DrawButtonGrid(saveXml);
            } else {
                XmlDocument saveXml = new XmlDocument();
                saveXml.Load(xmlPath);

                DrawButtonGrid(saveXml);
            }
        }

        private void DrawButtonGrid(XmlDocument saveXml) {
            i = 0;
            DynamicButtonGrid.Children.Clear();

            foreach (XmlNode buttons in saveXml) {
                foreach (XmlNode button in buttons.ChildNodes) {
                    System.Windows.Controls.Button newBtn = new System.Windows.Controls.Button {
                        Content = defaultButtonText,
                        Width = buttonWidth,
                        Height = buttonHeight,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, (i * buttonHeight), 0, 0),
                        Name = button.Name
                    };

                    System.Windows.Controls.TextBox newTxb = new System.Windows.Controls.TextBox {
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = textBoxWidth,
                        Height = textBoxHeight,
                        Margin = new Thickness(buttonWidth, (i * textBoxHeight), 0, 0),
                        IsReadOnly = true,
                        Text = button.InnerText
                    };

                    System.Windows.Controls.Button removeButton = new System.Windows.Controls.Button {
                        Content = "Remove",
                        Width = buttonWidth,
                        Height = buttonHeight,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness((buttonWidth + textBoxWidth), (i * buttonHeight), 0, 0),
                        Name = button.Name
                    };

                    DynamicButtonGrid.Children.Add(newBtn);
                    DynamicButtonGrid.Children.Add(newTxb);
                    DynamicButtonGrid.Children.Add(removeButton);
                    i++;

                    newBtn.Click += OnButtonClicked;
                    removeButton.Click += OnRemoveClicked;
                }
            }

            xmlDoc = saveXml;

            DrawConfigButtons(i);
        }

        private void DrawConfigButtons(int btnAmount) {
            //Add Path
            System.Windows.Controls.Button addPath = new System.Windows.Controls.Button {
                Content = "Add Path",
                Width = buttonWidth,
                Height = buttonHeight,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, (btnAmount * buttonHeight), 0, 0)
            };

            DynamicButtonGrid.Children.Add(addPath);
            addPath.Click += AddPath;

            SizeToContent = SizeToContent.Height;
        }

        private void AddPath(object sender, RoutedEventArgs e) {
            foreach (XmlNode buttons in xmlDoc) {
                if (buttons.Name == xmlButtons) {
                    XmlNode newBtn = xmlDoc.CreateNode("element", buttonNamePrefix + i.ToString(), "");
                    newBtn.InnerText = "File Path";
                    buttons.AppendChild(newBtn);
                    break;
                }
            }

            xmlDoc.Save(xmlPath);
            DrawButtonGrid(xmlDoc);
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Button clickedButton = (System.Windows.Controls.Button)sender;

            FolderBrowserDialog folderBrowseDialog = new FolderBrowserDialog();
            folderBrowseDialog.ShowDialog();

            foreach (XmlNode buttons in xmlDoc)
                foreach (XmlNode button in buttons.ChildNodes)
                    if (button.Name == clickedButton.Name)
                        button.InnerText = folderBrowseDialog.SelectedPath;

            xmlDoc.Save(xmlPath);
            DrawButtonGrid(xmlDoc);
        }

        private void OnRemoveClicked(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Button clickedButton = (System.Windows.Controls.Button)sender;

            foreach (XmlNode buttons in xmlDoc)
                foreach (XmlNode button in buttons.ChildNodes)
                    if (button.Name == clickedButton.Name)
                        buttons.RemoveChild(button);

            xmlDoc.Save(xmlPath);
            DrawButtonGrid(xmlDoc);
        }
    }
}
