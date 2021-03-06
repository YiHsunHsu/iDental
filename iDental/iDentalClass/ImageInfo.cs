﻿using System;
using System.Windows.Media.Imaging;

namespace iDental.iDentalClass
{
    public class ImageInfo :ViewModels.ViewModelBase.PropertyChangedBase
    {
        public DateTime Registration_Date { get; set; }
        public int Image_ID { get; set; }
        public string Image_Path { get; set; }
        public string Image_FullPath { get; set; }
        public string Image_FileName { get; set; }
        public string Image_Extension { get; set; }
        public int Registration_ID { get; set; }
        public DateTime CreateDate { get; set; }

        private BitmapImage bitmapImage;
        public BitmapImage BitmapImage
        {
            get { return bitmapImage; }
            set
            {
                bitmapImage = value;
                OnPropertyChanged("BitmapImage");
            }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        private bool isChecked = false;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
    }
}
