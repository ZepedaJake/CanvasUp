using System;
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CanvasUp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    //NOTE. Bitmaps are Top left to bottom right
    //[0,0] [1,0] [2,0]
    //[0,1] [1,1] [2,2]
    //[0,2] [1,2] [2,2]
    public partial class MainWindow : Window
    {
        Bitmap importBitmap;
        Bitmap editedBitmap;
        Bitmap stitchMap;
        string firstFile = "";
        string newFile = "";
        System.Drawing.Color backgroundColor = new System.Drawing.Color(); //used to be System.Drawing.Color.FromArgb(0, 0, 0), but thats only good for black backgrounds
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LstPictures_PreviewDrop(object sender, DragEventArgs e)
        {
            //gets the files as a string array
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //for each file, add it to the list of attachments if it is not there yet.
            foreach (string filename in files)
            {
                if (lstPictures.Items.Count > 0)
                {
                    bool shouldAddAttachment = true;
                    foreach (ListBoxItem lbi in lstPictures.Items)
                    {
                        //look through the list box to see if the file is attached already, if so, shouldAddAttachment will be set to false
                        if (lbi.Content.ToString() == filename)
                        {
                            //do nothing. this item is already attached
                            shouldAddAttachment = false;
                            break;
                        }

                    }

                    if (shouldAddAttachment)
                    {
                        ListBoxItem lstItem = new ListBoxItem();
                        lstItem.Content = filename;
                        lstPictures.Items.Add(lstItem);
                    }
                }
                else
                {
                    //there are no items, add one;
                    ListBoxItem lstItem = new ListBoxItem();
                    lstItem.Content = filename;
                    lstPictures.Items.Add(lstItem);
                }
            }
        }

        private void LstPictures_PreviewDragEnter(object sender, DragEventArgs e)
        {

        }

        string CheckForFile(string checkFile)
        {
            string useThisFileName = checkFile;
            while (File.Exists(checkFile))
            {
                checkFile = checkFile.Insert(checkFile.Length - 4, "_Copy");
                useThisFileName = checkFile;
            }          

            return useThisFileName;
        }
        private void BtnResize_Click(object sender, RoutedEventArgs e)
        {
            //System.Drawing.Color cBlack = System.Drawing.Color.FromArgb(0,0,0);
            firstFile = "";
            newFile = "";

            int newWidth = int.Parse(txtNewWdith.Text);
            int newHeight = int.Parse(txtNewHeight.Text);
            stitchMap = new Bitmap(lstPictures.Items.Count * newWidth, newHeight);
            int index = 0;

            foreach(ListBoxItem l in lstPictures.Items)
            {
                string file = l.Content.ToString();
                
                if(chkStitch.IsChecked == false)
                {
                    newFile = CheckForFile(file.Insert(file.Length - 4, "_Edit"));
                }
                else
                {
                    if (firstFile == "")
                    {
                        firstFile = CheckForFile(file.Insert(file.Length - 4, "_Stitched"));
                    }

                }

                int xChange = 0;
                int yChange = 0;
                
                
                importBitmap = new Bitmap(@file);
                editedBitmap = new Bitmap(newWidth, newHeight);
                backgroundColor = importBitmap.GetPixel(0, 0);

                //find difference of size and start drawing the new bitmap
                xChange = editedBitmap.Width - importBitmap.Width;
                yChange = editedBitmap.Height - importBitmap.Height;               

                //just draw entire bitmap as black
                for (int x = 0; x < editedBitmap.Width; x++)
                {
                    for(int y = 0; y < editedBitmap.Height; y++)
                    {
                        editedBitmap.SetPixel(x, y,backgroundColor);
                    }
                }

                int a = 0; //imported bitmap X tracker
                int b = 0; //imported  bitmap Y tracker
                int h = newHeight;
                int w = newWidth;
                int eraseLeft = 0;//how many left columns available to erase
                int doEraseLeft = 0;//how many to erase after calculating
                bool emptyLeft = false;//loop should add on to eraseleft

                int eraseRight = 0;
                int doEraseRight = 0;
                bool emptyRight = false;

                int eraseTop = 0;
                bool emptyTop = false;
                int doEraseTop = 0;

                int eraseBottom = 0;
                bool emptyBottom = false;

                if(xChange > -1 && yChange > -1) //for images smaller than target size----------------------------------------------------------------------------------------------------------------
                {
                    for (int x = (int)Math.Ceiling((double)xChange / 2); x < ((int)Math.Ceiling((double)xChange / 2) + importBitmap.Width); x++)
                    {
                        b = 0;
                        for (int y = yChange; y < editedBitmap.Height; y++)
                        {

                            editedBitmap.SetPixel(x, y, importBitmap.GetPixel(a, b));
                            b++;
                        }
                        a++;
                    }                   
                }
                else if(xChange < 0 && yChange > -1)//images wider than target, but shorter than target-------------------------------------------------------------------------------
                {
                    for (int x = 0; x < importBitmap.Width; x++)//sweep left to right, if you hit a colored pixel stop
                    {
                        for (int y = 0; y < importBitmap.Height; y++)
                        {
                            emptyLeft = false;
                            //if (importBitmap.GetPixel(x, y) != System.Drawing.Color.FromArgb(0, 0, 0))
                            if (importBitmap.GetPixel(x, y) != backgroundColor)
                            {
                                break;
                            }
                            else
                            {
                                emptyLeft = true;
                            }
                        }

                        if (emptyLeft)
                        {
                            eraseLeft++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int x = importBitmap.Width - 1; x > -1; x--)//sweep right to left, if you hit a colored pixel stop
                    {
                        for (int y = 0; y < importBitmap.Height; y++)
                        {
                            emptyRight = false;
                            //if (importBitmap.GetPixel(x, y) != System.Drawing.Color.FromArgb(0, 0, 0))
                            if (importBitmap.GetPixel(x, y) != backgroundColor)
                            {
                                break;
                            }
                            else
                            {
                                emptyRight = true;
                            }
                        }

                        if (emptyRight)
                        {
                            eraseRight++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int x = xChange; xChange < 0;) //xChange will be negative since we are shrinking the picture
                    {
                        if (xChange < 0 && eraseRight <= 0 && eraseLeft <= 0)
                        {
                            MessageBox.Show("Unable to erase black spaces from the sides");
                            return;
                        }

                        if (xChange < 0 && eraseLeft > 0)
                        {
                            eraseLeft--;
                            doEraseLeft++;
                            xChange++;
                        }

                        if (xChange < 0 && eraseRight > 0)
                        {
                            eraseRight--;
                            doEraseRight++;
                            xChange++;
                        }
                    }

                    for (int x = doEraseLeft; x < editedBitmap.Width + doEraseLeft; x++)//bigger portions get the x and y, smaller get the a and b
                    {
                        b = 0;
                        for (int y = yChange; y < editedBitmap.Height; y++)
                        {

                            editedBitmap.SetPixel(a, y, importBitmap.GetPixel(x, b));
                            b++;
                        }
                        a++;
                    }
                }
                else if(xChange > -1 && yChange < 0)//images taller than target, but thinner than target------------------------------------------------------------------------------------
                {
                    for (int y = 0; y < importBitmap.Height; y++)//sweep top to bottom, if you hit a colored pixel stop
                    {
                        for (int x = 0; x < importBitmap.Width; x++)
                        {
                            emptyTop = false;
                            if (importBitmap.GetPixel(x, y) != backgroundColor)
                            {
                                break;
                            }
                            else
                            {
                                emptyTop = true;
                            }
                        }

                        if (emptyTop)
                        {
                            eraseTop++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (importBitmap.Height - eraseTop > editedBitmap.Height)//if the image still needs cutting after removing everything possible from the top
                    {
                        for (int y = importBitmap.Height - 1; y > -1; y--)//sweep bottom to top, if you hit a colored pixel stop
                        {
                            for (int x = 0; x < importBitmap.Width; x++)
                            {
                                emptyBottom = false;
                                if (importBitmap.GetPixel(x, y) != backgroundColor)
                                {
                                    break;
                                }
                                else
                                {
                                    emptyBottom = true;
                                }
                            }

                            if (emptyBottom)
                            {
                                eraseBottom++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    for (int y = yChange; yChange < 0;)//yChange will be negative since we are shrinking the picture
                    {
                        if (yChange < 0 && eraseTop > 0)
                        {
                            eraseTop--;
                            doEraseTop++;
                            yChange++;
                        }
                        else if (yChange < 0 && eraseBottom > 0)
                        {
                            eraseBottom--;
                            yChange++;
                        }
                        else
                        {
                            MessageBox.Show("Unable to erase black space from the top");
                            return;
                        }
                    }

                    for (int x = (int)Math.Ceiling((double)xChange / 2); x < ((int)Math.Ceiling((double)xChange / 2) + importBitmap.Width); x++)//bigger portions get the x and y, smaller get the a and b
                    {
                        b = 0;
                        for (int y = doEraseTop; y < editedBitmap.Height + doEraseTop; y++)
                        {
                            editedBitmap.SetPixel(x, b, importBitmap.GetPixel(a, y));
                            b++;
                        }
                        a++;
                    }
                }
                else if(xChange < 0 && yChange < 0)//images taller and wider than target-----------------------------------------------------------------------------------------------------
                {
                    for (int x = 0; x < importBitmap.Width; x++)//sweep left to right, if you hit a colored pixel stop
                    {
                        for (int y = 0; y < importBitmap.Height; y++)
                        {
                            emptyLeft = false;                       
                            if (importBitmap.GetPixel(x, y) != backgroundColor)
                            {
                                break;
                            }
                            else
                            {
                                emptyLeft = true;
                            }
                        }

                        if (emptyLeft)
                        {
                            eraseLeft++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int x = importBitmap.Width - 1; x > -1; x--)//sweep right to left, if you hit a colored pixel stop
                    {
                        for (int y = 0; y < importBitmap.Height; y++)
                        {
                            emptyRight = false;
                            if (importBitmap.GetPixel(x, y) != backgroundColor)
                            {
                                break;
                            }
                            else
                            {
                                emptyRight = true;
                            }
                        }

                        if (emptyRight)
                        {
                            eraseRight++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int y = 0; y < importBitmap.Height; y++)//sweep top to bottom, if you hit a colored pixel stop
                    {
                        for (int x = 0; x < importBitmap.Width; x++)
                        {
                            emptyTop = false;
                            if (importBitmap.GetPixel(x, y) != backgroundColor)
                            {
                                break;
                            }
                            else
                            {
                                emptyTop = true;
                            }
                        }

                        if (emptyTop)
                        {
                            eraseTop++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (importBitmap.Height - eraseTop > editedBitmap.Height)//if the image still needs cutting after removing everything possible from the top
                    {
                        for (int y = importBitmap.Height - 1; y > -1; y--)//sweep bottom to top, if you hit a colored pixel stop
                        {
                            for (int x = 0; x < importBitmap.Width; x++)
                            {
                                emptyBottom = false;
                                if (importBitmap.GetPixel(x, y) != backgroundColor)
                                {
                                    break;
                                }
                                else
                                {
                                    emptyBottom = true;
                                }
                            }

                            if (emptyBottom)
                            {
                                eraseBottom++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    for (int x = xChange; xChange < 0;) //xChange will be negative since we are shrinking the picture
                    {
                        if (xChange < 0 && eraseRight <= 0 && eraseLeft <= 0)
                        {
                            MessageBox.Show("Unable to erase black spaces from the sides");
                            return;
                        }

                        if (xChange < 0 && eraseLeft > 0)
                        {
                            eraseLeft--;
                            doEraseLeft++;
                            xChange++;
                        }

                        if (xChange < 0 && eraseRight > 0)
                        {
                            eraseRight--;
                            doEraseRight++;
                            xChange++;
                        }
                    }

                    for (int y = yChange; yChange < 0;)//yChange will be negative since we are shrinking the picture
                    {
                        if (yChange < 0 && eraseTop > 0)
                        {
                            eraseTop--;
                            doEraseTop++;
                            yChange++;
                        }
                        else if (yChange < 0 && eraseBottom > 0)
                        {
                            eraseBottom--;
                            yChange++;
                        }
                        else
                        {
                            MessageBox.Show("Unable to erase black space from the top");
                            return;
                        }
                    }

                    
                    for (int x = doEraseLeft; x < editedBitmap.Width + doEraseLeft; x++)//bigger portions get the x and y, smaller get the a and b
                    {
                        b = 0;
                        for (int y = doEraseTop; y < editedBitmap.Height + doEraseTop; y++)
                        {
                            editedBitmap.SetPixel(a, b, importBitmap.GetPixel(x, y));
                            b++;
                        }
                        a++;
                    }
                }
                
                if (chkStitch.IsChecked == true)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        for (int y = 0; y < newHeight; y++)
                        {
                            stitchMap.SetPixel((x + (index * newWidth)), y, editedBitmap.GetPixel(x, y));
                        }
                    }
                    index++;
                }
                else
                {
                    editedBitmap.Save(newFile, ImageFormat.Png);
                }
            }

            if(chkStitch.IsChecked == true)
            {
                stitchMap.Save(firstFile, ImageFormat.Png);
            }

            MessageBox.Show("Finished");
        }

        private void btnClearList_Click(object sender, RoutedEventArgs e)
        {
            lstPictures.Items.Clear();
        }
    }
}
