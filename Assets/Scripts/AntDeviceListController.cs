using SmallEarthTech.AntPlus;
using SmallEarthTech.AntPlus.Extensions.Hosting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class AntDeviceListController
{
    private AntCollection _devices;
    private bool _updateList, _clearDetails;
    private readonly object _deviceListLock = new();

    // UI element references
    private readonly ListView _antDeviceListView;
    private readonly Label _deviceClassLabel;
    private readonly Label _deviceNameLabel;
    private readonly VisualElement _devicePortrait;

    public AntDeviceListController(VisualElement root)
    {
        _antDeviceListView = root.Q<ListView>("device-list");

        // Store references to the selected character info elements
        _deviceClassLabel = root.Q<Label>("device-class");
        _deviceNameLabel = root.Q<Label>("device-name");
        _devicePortrait = root.Q<VisualElement>("device-portrait");

        _antDeviceListView.fixedItemHeight = 45;
        _antDeviceListView.itemsSource = new List<AntDevice>();
        _antDeviceListView.selectionChanged += OnDeviceSelected;

        _antDeviceListView.bindItem = (item, index) =>
        {
            Debug.Log($"bindItem: index = {index}");
            item.Q<Label>("device-name").text = _antDeviceListView.itemsSource[index].ToString();
        };

        _antDeviceListView.unbindItem = (item, index) =>
        {
            Debug.Log($"unbindItem: index = {index}");
        };
        _antDeviceListView.destroyItem = (item) => { Debug.Log($"destroyItem: item = {item}"); };
    }

    public void InitializeAntDeviceList(AntCollection antDevices)
    {
        _devices = antDevices;
        _devices.CollectionChanged += Devices_CollectionChanged;
    }

    public void Update()
    {
        lock (_deviceListLock)
        {
            if (_updateList)
            {
                Debug.Log($"Update: update list - _antDeviceListView count = {_antDeviceListView.itemsSource.Count}");
                _antDeviceListView.RefreshItems();
                _updateList = false;
            }
            if (_clearDetails)
            {
                Debug.Log("Update: clear details");
                _antDeviceListView.ClearSelection();
                _clearDetails = false;
            }
        }
    }

    private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        lock (_deviceListLock)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _antDeviceListView.itemsSource.Add((AntDevice)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _clearDetails = e.OldItems[0] == _antDeviceListView.selectedItem;
                    _antDeviceListView.itemsSource.Remove((AntDevice)e.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
            Debug.Log($"Devices_CollectionChanged: _antDeviceListView count = {_antDeviceListView.itemsSource.Count}, _clearDetails = {_clearDetails}");
            _updateList = true;
        }
    }

    private void OnDeviceSelected(IEnumerable<object> enumerable)
    {
        AntDevice selectedDevice = (AntDevice)_antDeviceListView.selectedItem;
        Debug.Log($"OnDeviceSelected: selectedDevice = {selectedDevice}");

        // Handle none-selection (Escape to deselect everything)
        if (selectedDevice == null)
        {
            // Clear character details
            Debug.Log("OnDeviceSelected: clear details");
            _deviceClassLabel.text = "";
            _deviceNameLabel.text = "";
            _devicePortrait.style.backgroundImage = null;
        }
        else
        {
            // Fill in character details
            Debug.Log("OnDeviceSelected: fill details");
            _deviceClassLabel.text = selectedDevice.ChannelId.DeviceNumber.ToString();
            _deviceNameLabel.text = selectedDevice.ToString();
            _devicePortrait.style.backgroundImage = new StyleBackground(LoadImageFromAntDevice(selectedDevice));
        }
    }

    private Sprite LoadImageFromAntDevice(AntDevice antDevice)
    {
        // Assuming 'stream' is your Stream object containing the image data
        Texture2D texture = new(120, 120);
        byte[] imageData;

        using (MemoryStream ms = new())
        {
            antDevice.DeviceImageStream.CopyTo(ms);
            imageData = ms.ToArray();
        }

        texture.LoadImage(imageData); // Load the image data into the texture

        // Now you can use the texture in your Unity objects, for example:
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
