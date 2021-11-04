//#define LocalTest
using System.IO;
using System.Windows.Media.Imaging;
using WFInventory.Cloud;

using ImageMagick;
using System.Windows.Input;
using WFInventory;
using System.Collections.Generic;
using System;
using WFInventory.ImageEffects;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

using System.Diagnostics;
using FSCommon;

namespace WFInventory.ViewModels
{
    [DocumentVM]
    public partial class wf_ImageEditDocumentViewModel : DocumentViewModel
    {
        enum EffectInext
        {
            Crop
        }

        private wf_ImageEdit _data;

        private FileDownloadInfo thumbnaildownload;
        private FileDownloadInfo orginalImageDownload;
        private FileDownloadInfo currentImageDownload;

        private void placeHolder(object obj)
        {
            //Call model functions on documents
            //_data.placeHolderFunction();
        }
        public wf_ImageEditDocumentViewModel(FirestoreNode document) : base(document)
        {
            _data = (wf_ImageEdit)document;
            //hasContextMenu = false;
            ////ContextMenuItems.Add(new vmMenuItem("MenuItemTitle", placeHolder, commandEnabled));

            Application.Current.Dispatcher?.BeginInvoke(new Action(() =>
                   {
                       ImageEffectBase baseimage = new ImageEffectBase(this);
                       string effectData = EffectData;

                       ImageEffectChain.Add(baseimage);
                       string[] effects = effectData.Split('&');
                       ImageEffectBase lasteffect = baseimage;
                       foreach (string effect in effects)
                       {
                           string[] effectdecode = effect.Split('=');
                           if (effectdecode.Length == 1)
                           {
                               ImageEffectChain.Add(ImageUtil.ImageEffectFactory(ImageEffectChain.Last(), "error"));
                           }
                           else
                           {
                               ImageEffectChain.Add(ImageUtil.ImageEffectFactory(ImageEffectChain.Last(), effectdecode[0], effectdecode[1]));
                           }

                       }
                   }));
            AddEffectCmd = new relayCommand(commandEnabled, AddEffect);
            DeleteEffectCmd = new relayCommand(commandEnabled, DeleteEffect);
            SaveCurrentEditCmd = new relayCommand(commandEnabled, SaveCurrentEdit);
        }

        public string EffectData { get => _data.EffectData; set => _data.EffectData = value; }

        public ICommand AddEffectCmd { get; private set; }
        public ICommand DeleteEffectCmd { get; private set; }

        public ICommand SaveCurrentEditCmd { get; private set; }

        public void AddEffect(object o)
        {
            if (SelectedEffectId != "0")
            {
                ImageEffectChain.Add(ImageUtil.ImageEffectFactory(ImageEffectChain.Last(), SelectedEffectId));
                SelectedEffect = ImageEffectChain.Last();
                onPropertyChanged("EffectChainOutput");
            }
        }


        public string EditedFileLocation { get => _data.EditedFileLocation; set => _data.EditedFileLocation = value; }
        public string ThumbnailFileLocation { get => _data.ThumbnailFileLocation; set => _data.ThumbnailFileLocation = value; }
          

        public void SaveCurrentEdit(object o)
        {
            string ed = "";
            foreach (var effect in ImageEffectChain)
            {
                if (ed != "") ed += "&";
                ed += ImageUtil.GenerateSaveString(effect);
            }
            //save current effect chain
            EffectData = ed;


            //upload new image to server...
            string tempfilename = "tempfile" + new Random().Next(100000000, 999999999).ToString();

           
            LastEffectInChain.Output.Write(tempfilename);
            currentupload = new FileUploadInfo(tempfilename, "imglibrary", EditedFileLocation);
            CS.TransferQueue.Enqueue(currentupload);
            currentupload.PropertyChanged += currentupload_PropertyChanged;
            MagickImage t = ((MagickImage)LastEffectInChain.Output.Clone());
            t.Scale(ImageUtil.DefaultThumbnailSize, ImageUtil.DefaultThumbnailSize);
            string tempfilename2 = "tempfile" + new Random().Next(100000000, 999999999).ToString();
            t.Write(tempfilename2);
            thumbnailupload = new FileUploadInfo(tempfilename2, "imglibrary", ThumbnailFileLocation);
            CS.TransferQueue.Enqueue(thumbnailupload);
            thumbnailupload.PropertyChanged += thumbnailupload_PropertyChanged;
            _data.RequiresPublish = true;
        }


        FileUploadInfo thumbnailupload;
        FileUploadInfo currentupload;

        //public bool MenuImage { get => _data.MenuImage; }

        private void thumbnailupload_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Complete")
            {
                if (thumbnailupload.Complete == true)
                {
                    thumbnaildownload = null;
                    onPropertyChanged("Thumbnail");
                    thumbnailupload.PropertyChanged -= thumbnailupload_PropertyChanged;
                }
            }
        }

        private void currentupload_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Complete")
            {
                if (currentupload.Complete == true)
                {
                    currentImageDownload = null;
                    onPropertyChanged("CurrentImage");
                    onPropertyChanged("BitmapOutput");
                    currentupload.PropertyChanged -= currentupload_PropertyChanged;
                }
            }
        }

        public void DeleteEffect(object o)
        {
            ImageEffectBase effect = (ImageEffectBase)o;
            if (ImageEffectChain.Last() != effect)
            {
                ImageEffectBase neffect = ImageEffectChain.Last();
                while (neffect.Source != effect) neffect = neffect.Source;
                neffect.Source = effect.Source;
            }

            ImageEffectChain.Remove(effect);
            onPropertyChanged("EffectChainOutput");
        }

        private void Thumbnaildownload_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Complete")
            {
                if (thumbnaildownload.Complete == true)
                {
                    thumbnaildownload.PropertyChanged -= Thumbnaildownload_PropertyChanged;
                    thumbnaildownload.DownloadedFileStream.Position = 0;
                    MIThumbnail = new MagickImage(thumbnaildownload.DownloadedFileStream);
                    thumbnaildownload.DownloadedFileStream.Dispose();
                    _thumbail = MIThumbnail.ToBitmapSource();
                    _thumbail.Freeze();
                    onPropertyChanged("Thumbnail");
                }
            }
        }

        private void CurrentImageDownload_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Complete")
            {
                if (currentImageDownload.Complete == true)
                {
                    currentImageDownload.PropertyChanged -= CurrentImageDownload_PropertyChanged;
                    currentImageDownload.DownloadedFileStream.Position = 0;
                    MICurrent = new MagickImage(currentImageDownload.DownloadedFileStream);
                    _currentImage = MICurrent.ToBitmapSource();
                    _currentImage.Freeze();
                    onPropertyChanged("CurrentImage");
                    onPropertyChanged("BitmapOutput");
                }
            }
        }

        private void OrginalImageDownload_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Complete")
            {
                if (orginalImageDownload.Complete == true)
                {
                    orginalImageDownload.PropertyChanged -= OrginalImageDownload_PropertyChanged;
                    orginalImageDownload.DownloadedFileStream.Position = 0;
                    MIOriginal = new MagickImage(orginalImageDownload.DownloadedFileStream);
                    orginalImageDownload.DownloadedFileStream.Dispose();
                    _originalImage = MIOriginal.ToBitmapSource();
                    _originalImage.Freeze();
                    onPropertyChanged("OriginalImage");
                    onPropertyChanged("BitmapInput");
                }
            }
        }

        public new string Name
        {
            get => $"{_data.OriginalFileLocation}";
        }

        private BitmapSource _thumbail = ImageUtil.LoadingSmall;
        private BitmapSource _originalImage = ImageUtil.LoadingSmall;
        private BitmapSource _currentImage = ImageUtil.LoadingSmall;

        public MagickImage MIThumbnail { get; set; }
        public MagickImage MIOriginal { get; set; }
        public MagickImage MICurrent { get; set; }

        public Canvas OutputCanvas { get; set; }
        public Canvas InputCanvas { get; set; }

        public BitmapSource Thumbnail
        {
            get
            {
                if (thumbnaildownload == null)
                {
                    thumbnaildownload = new FileDownloadInfo("imglibrary", _data.ThumbnailFileLocation);
                    thumbnaildownload.PropertyChanged += Thumbnaildownload_PropertyChanged;
                    CS.TransferQueue.Enqueue(thumbnaildownload);
                }
                return _thumbail;
            }
        }

        public Dictionary<string, string> EffectList { get => ImageUtil.EffectList; }

        public string SelectedEffectId { get; set; } = "0";

        public BitmapSource OriginalImage
        {
            get
            {
                if (orginalImageDownload == null)
                {
                    orginalImageDownload = new FileDownloadInfo("imglibrary", _data.OriginalFileLocation);
                    orginalImageDownload.PropertyChanged += OrginalImageDownload_PropertyChanged;
                    CS.TransferQueue.Enqueue(orginalImageDownload);
                }
                return _originalImage;
            }
        }

        public bool OriginalImageLoaded { get; private set; }

        public bool RequiresPublish { get => _data.RequiresPublish; set => _data.RequiresPublish = value; }

        public BitmapSource CurrentImage
        {
            get
            {
                if (currentImageDownload == null)
                {
                    currentImageDownload = new FileDownloadInfo("imglibrary", _data.EditedFileLocation);
                    currentImageDownload.PropertyChanged += CurrentImageDownload_PropertyChanged;
                    CS.TransferQueue.Enqueue(currentImageDownload);

                }
                return _currentImage;
            }
        }

        public string PrintedScale { get => _data.PrintedSize.ToString(); set => _data.PrintedSize = Convert.ToDouble(value); }

        public ObservableCollection<ImageEffectBase> ImageEffectChain { get; } = new ObservableCollection<ImageEffectBase>();

        private ImageEffectBase _selectedEffect = null;
        public ImageEffectBase SelectedEffect { get => _selectedEffect; set {
                if (_selectedEffect != null)
                {
                    _selectedEffect.UiOff();
                    _selectedEffect.PropertyChanged -= _selectedEffect_PropertyChanged;
                }
                _selectedEffect = value;
                if (_selectedEffect != null)
                {
                    _selectedEffect.UiOn(InputCanvas, OutputCanvas);
                    _selectedEffect.PropertyChanged += _selectedEffect_PropertyChanged;
                }
                onPropertyChanged("SelectedEffect");
                onPropertyChanged("BitmapOutput");
                onPropertyChanged("BitmapInput");
            } }

        private void _selectedEffect_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BitmapPreviewOutput") onPropertyChanged("BitmapOutput");
            if (e.PropertyName == "BitmapInput") onPropertyChanged("BitmapInput");
        }

        public BitmapSource BitmapOutput
        {
            get
            {
                if (SelectedEffect == null) return CurrentImage;
                else return SelectedEffect.BitmapPreviewOutput;
            }
        }

        public BitmapSource BitmapInput
        {
            get
            {
                if (SelectedEffect == null) return OriginalImage;
                else return SelectedEffect.BitmapInput;
            }
        }

        public ImageEffectBase _lastEffectInChain = null;

        public ImageEffectBase LastEffectInChain {

            get {
                if (ImageEffectChain.Count > 0)
                {
                    if (_lastEffectInChain != null)
                    {
                        _lastEffectInChain.PropertyChanged -= LastImageffectPropertyChanged;
                    }
                    _lastEffectInChain = ImageEffectChain.Last();
                    _lastEffectInChain.PropertyChanged += LastImageffectPropertyChanged;
                    return _lastEffectInChain;
                }
                return null;
            }
        }

        public BitmapSource EffectChainOutput
        {
            get => LastEffectInChain.BitmapOutput;
        }

        private void LastImageffectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BitmapOutput")
            {
                onPropertyChanged("EffectChainOutput");
            }
        }

        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEffect != null)
            {
                SelectedEffect.MouseDown(sender, e);
            }
        }

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEffect != null)
            {
                SelectedEffect.MouseUp(sender, e);
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedEffect != null)
            {
                SelectedEffect.MouseMove(sender, e);
            }
        }

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (SelectedEffect != null)
            {
                SelectedEffect.MouseWheel(sender, e);
            }
        }


        public void KeyDown(object sender, KeyEventArgs e)
        {
            if (SelectedEffect != null)
            {
                SelectedEffect.KeyDown(sender, e);
            }
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            if (SelectedEffect != null)
            {
                SelectedEffect.KeyUp(sender, e);
            }
        }


        public async Task PublishWebImages(string remotebase, Dictionary<string, double> sizes = null)
        {
            StdOut.WriteLine("UpdateImages", $"Publishing: {remotebase}");

            var i = CurrentImage;
            while (currentImageDownload == null || currentImageDownload.Complete == false || MICurrent == null)
            {
                await Task.Delay(1000);
            }

            Random gen = new Random();
            string tempfilebase = remotebase.Replace('/', '-') + gen.Next(1000000, 9999999);
            //tempfilebase = "NotFound";

            MagickImage imp = (MagickImage)MICurrent.Clone();

            byte dc = 44;
            byte lc = 250;
            float dcf = 44;
            float lcf = 250;
            var darkcolour = MagickColor.FromRgb(44, 44, 44);
            var lightcolour = MagickColor.FromRgb(250, 250, 250);

            //imp.Scale(new MagickGeometry(250, 250));
            //imp.Format = MagickFormat.Jpg;
            //imp.Write(ImageUtil.TempImageDirectory + tempfilebase + "-Thumb.jpg", MagickFormat.Jpg);

            imp.Scale(new MagickGeometry(2000, 2000));
            imp.Strip();
            //Debug.WriteLine($"--");
            //Debug.WriteLine($"{this.Id}");
            //Debug.WriteLine($"{imp.ColorSpace.ToString()}");
            //if (imp.GetColorProfile() != null)
            //{
            //    IColorProfile ic = imp.GetColorProfile();
            //    Debug.WriteLine($"{imp.GetColorProfile().Description}");
            //}

            //imp.TransformColorSpace(ColorProfile.SRGB);
            //            imp.RemoveProfile("icc");
            imp.ColorSpace = ColorSpace.sRGB;



            //imp.Format = MagickFormat.Png24;
            //var pixels = darkimgbase.GetPixels();
            //CreateBorder(darkimgbase.GetPixels(), 100, dcf);
            //CreateBorder(lightimgbase.GetPixels(), 100, lcf);

            //var mask = imp.Clone();
            //mask.Format = MagickFormat.Png;
            //mask.Colorize(darkcolour, new Percentage(100));
            //mask.Shave(50, 50);
            //mask.BorderColor = new MagickColor(dc, dc, dc, 0);
            //mask.Border(50, 50);
            //mask.Blur(0, 10);
            //mask.Write(ImageUtil.TempImageDirectory + "DarkMask.png", MagickFormat.Png);
            //mask.Write(ImageUtil.TempImageDirectory + "DarkMask.jpg", MagickFormat.Jpg);


            //var lmask = imp.Clone();
            ////lmask.Format = MagickFormat.Png;
            //lmask.Colorize(lightcolour, new Percentage(100));
            //lmask.Shave(50, 50);
            //lmask.BorderColor = new MagickColor(lc, lc, lc, 0);
            //lmask.Border(50, 50);
            //lmask.Blur(0, 10);
            //lmask.Write(ImageUtil.TempImageDirectory + "LightMask.png", MagickFormat.Png);
            //lmask.Write(ImageUtil.TempImageDirectory + "LightMask.jpg", MagickFormat.Jpg);
            IMagickImage SelectedWatermark = null;


            double basecount = ImageUtil.SumOfImage(imp);
            double difference = 0;

            foreach (var img in ImageUtil.MIPossibleWatermarks2000)
            {
                MagickImage test = (MagickImage)imp.Clone();
                test.Composite(img, CompositeOperator.Over);
                double currentsum = ImageUtil.SumOfImage(test);

                if (Math.Abs(basecount - currentsum) > difference)
                {
                    SelectedWatermark = img;
                    difference = Math.Abs(basecount - currentsum);
                }
                test.Dispose();
            }

            //Debug.WriteLine(SelectedWatermark.FileName);
            var lightimgbase = imp.Clone();
            var darkimgbase = imp.Clone();
            imp.Dispose();

            lightimgbase.Composite(SelectedWatermark, CompositeOperator.Over);
            darkimgbase.Composite(SelectedWatermark, CompositeOperator.Over);

            lightimgbase.BorderColor = darkcolour;
            lightimgbase.Shave(50, 50);
            lightimgbase.Border(50, 50);
            lightimgbase.Composite(ImageUtil.LightMask, CompositeOperator.CopyAlpha);
            lightimgbase.BackgroundColor = lightcolour;
            lightimgbase.Alpha(AlphaOption.Remove);

            darkimgbase.BorderColor = lightcolour;
            darkimgbase.Shave(50, 50);
            darkimgbase.Border(50, 50);
            darkimgbase.Composite(ImageUtil.DarkMask, CompositeOperator.CopyAlpha);
            darkimgbase.BackgroundColor = darkcolour;
            darkimgbase.Alpha(AlphaOption.Remove);

            List<int> imgexpsizes = new List<int>();
            imgexpsizes.Add(2000);

#if !LocalTest
            var lightoutofstock = lightimgbase.Clone();
            var darkoutofstock = darkimgbase.Clone();

            lightoutofstock.Composite(ImageUtil.MIOutOfStock2000, CompositeOperator.Over);
            darkoutofstock.Composite(ImageUtil.MIOutOfStock2000, CompositeOperator.Over);

            darkoutofstock.Scale(250, 250);
            darkoutofstock.Write(ImageUtil.TempImageDirectory + tempfilebase + "-Dark-OFS.jpg", MagickFormat.Jpg);
            uploadfile(new FileUploadInfo(ImageUtil.TempImageDirectory + tempfilebase + "-Dark-OFS.jpg", "imgserver", remotebase + "-Dark-OFS.jpg", true));
            lightoutofstock.Scale(250, 250);
            lightoutofstock.Write(ImageUtil.TempImageDirectory + tempfilebase + "-Light-OFS.jpg", MagickFormat.Jpg);
            uploadfile(new FileUploadInfo(ImageUtil.TempImageDirectory + tempfilebase + "-Light-OFS.jpg", "imgserver", remotebase + "-Light-OFS.jpg", true));
            darkoutofstock.Dispose();
            lightoutofstock.Dispose();

            var ruler = ImageUtil.ssBG.Clone();

            if (sizes != null)
            {
                //generate ruler...
                foreach (var kvp in sizes)
                {
                    string szid = kvp.Key;
                    double scale = kvp.Value;

                    if (scale > 0)
                    {

                        double rulerwidth = 1.5;
                        double metscale = scale * 2.54;
                        double ppcm = 2000 / metscale;
                        double ppi = 2000 / scale;

                        ruler.Density = new Density(ppi, ppi, DensityUnit.PixelsPerInch);
                        ruler.Crop((int)(rulerwidth / scale * 2000), 2000);

                        double basemmTickSpaceing = ppcm / 10;
                        double baseinTickSpaceing = ppi / 16;
                        double textnudge = ruler.Width / 100 * 4;

                        string mmpat = "fssssmssss";
                        string inpat = "fsesqseshsesqses";


                        Dictionary<char, double> mmth = new Dictionary<char, double>();

                        double middlespace = ruler.Width * 0.25;
                        double cmline = (ruler.Width - middlespace) / 2;
                        double inline = (ruler.Width - cmline);

                        mmth.Add('f', 1.0 * cmline);
                        mmth.Add('s', 0.0 * cmline);
                        mmth.Add('m', 0.5 * cmline);

                        Dictionary<char, double> inth = new Dictionary<char, double>();
                        inth.Add('f', (1.0 - 1.0) * cmline + inline);
                        inth.Add('e', (1.0 - 0.5) * cmline + inline);
                        inth.Add('s', (1.0 - 0.0) * cmline + inline);
                        inth.Add('q', (1.0 - 0.5) * cmline + inline);
                        inth.Add('h', (1.0 - 0.75) * cmline + inline);

                        //ruler.Draw(new DrawableStrokeWidth(0.5));

                        DrawableLine left = new DrawableLine(0, 0, 0, ruler.Height);
                        DrawableLine right = new DrawableLine(ruler.Width, 0, ruler.Width, ruler.Height);
                        ruler.Draw(left);
                        ruler.Draw(right);

                        DrawableLine cml = new DrawableLine(mmth['f'], 0, mmth['f'], ruler.Height);
                        ruler.Draw(cml);

                        DrawableLine inchline = new DrawableLine(inth['f'], 0, inth['f'], ruler.Height);
                        ruler.Draw(inchline);

                        DrawableLine mmline = new DrawableLine(mmth['s'], 0, mmth['s'], ruler.Height);
                        ruler.Draw(mmline);

                        DrawableLine eline = new DrawableLine(inth['e'], 0, inth['e'], ruler.Height);
                        ruler.Draw(eline);

                        DrawableLine sline = new DrawableLine(inth['s'], 0, inth['s'], ruler.Height);
                        ruler.Draw(sline);

                        double fontsize = ppi / 2 * scale;
                        ruler.Draw(new DrawableFontPointSize(fontsize));
                        for (int ci = 0; ci < scale; ci++)
                        {
                            int ct = 0;
                            foreach (char c in inpat)
                            {
                                double cy = baseinTickSpaceing * ct + ppi * ci;
                                DrawableLine tick = new DrawableLine(inth[c], cy, ruler.Width, cy);
                                ruler.Draw(tick);
                                ct++;
                            }
                            if (ci != 0)
                            {
                                ruler.Draw(new DrawableText(inline + textnudge, ppi * ci - textnudge, ci.ToString()));
                            }

                        }

                        fontsize = ppcm / 2 * scale;
                        ruler.Draw(new DrawableFontPointSize(fontsize));
                        for (int ccm = 0; ccm < metscale; ccm++)
                        {
                            int ct = 0;
                            foreach (char c in mmpat)
                            {
                                double cy = basemmTickSpaceing * ct + ppcm * ccm;
                                DrawableLine tick = new DrawableLine(0, cy, mmth[c], cy);
                                ruler.Draw(tick);
                                ct++;
                            }
                            if (ccm != 0)
                            {
                                Drawables d = new Drawables();
                                d.Text(cmline - textnudge, ppcm * ccm - textnudge, ccm.ToString());
                                d.TextAlignment(ImageMagick.TextAlignment.Right);
                                ruler.Draw(d);
                            }
                        }
                    }

                    var darkrul = darkimgbase.Clone();
                    darkrul.Composite(ruler, new PointD(100, 0));
                    darkrul.Write(ImageUtil.TempImageDirectory + tempfilebase + $"-Dark-{szid}.jpg", MagickFormat.Jpg);

                    var lightrul = lightimgbase.Clone();
                    lightrul.Composite(ruler, new PointD(100, 0));
                    lightrul.Write(ImageUtil.TempImageDirectory + tempfilebase + $"-Light-{szid}.jpg", MagickFormat.Jpg);

                    darkrul.Dispose();
                    lightrul.Dispose();
                    uploadfile(new FileUploadInfo(ImageUtil.TempImageDirectory + tempfilebase + $"-Dark-{szid}.jpg", "imgserver", remotebase + $"-Dark-{szid}.jpg", true));
                    uploadfile(new FileUploadInfo(ImageUtil.TempImageDirectory + tempfilebase + $"-Light-{szid}.jpg", "imgserver", remotebase + $"-Light-{szid}.jpg", true)); }

            }

            ruler.Dispose();




            imgexpsizes.Add(1000);
            imgexpsizes.Add(500);
            imgexpsizes.Add(250);
            imgexpsizes.Add(125);
#endif

            foreach (int sz in imgexpsizes)
            {
                darkimgbase.Scale(sz, sz);
                darkimgbase.Write(ImageUtil.TempImageDirectory + tempfilebase + $"-Dark-{sz}.jpg", MagickFormat.Jpg);
  

                lightimgbase.Scale(sz, sz);
                lightimgbase.Write(ImageUtil.TempImageDirectory + tempfilebase + $"-Light-{sz}.jpg", MagickFormat.Jpg);
#if !LocalTest
                uploadfile(new FileUploadInfo(ImageUtil.TempImageDirectory + tempfilebase + $"-Dark-{sz}.jpg", "imgserver", remotebase + $"-Dark-{sz}.jpg", true));
                uploadfile(new FileUploadInfo(ImageUtil.TempImageDirectory + tempfilebase + $"-Light-{sz}.jpg", "imgserver", remotebase + $"-Light-{sz}.jpg", true));
#endif
            }


            lightimgbase.Dispose();
            darkimgbase.Dispose();

            bool ready = false;
            while(!ready)
            {
                await Task.Delay(5000);
                ready = true;
                foreach(var fi in uploadingfiles)
                {
                    if(!fi.Complete)
                    {
                        ready = false;
                        continue;
                    }
                }
            }
            uploadingfiles.Clear();
            _data.RequiresPublish = false;
        }

        private void uploadfile(FileUploadInfo fi)
        {
            CS.TransferQueue.Enqueue(fi);
            uploadingfiles.Add(fi);
        }

        private List<FileUploadInfo> uploadingfiles = new List<FileUploadInfo>();



        public string ClickLink { get => _data.ClickLink; set => _data.ClickLink = value; }

    }
}
