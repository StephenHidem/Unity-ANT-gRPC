using SmallEarthTech.AntPlus;
using SmallEarthTech.AntPlus.Extensions.Hosting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class AntDeviceListController
{
    AntCollection devices;
    bool updateList, clearDetails;
    object deviceListLock = new object();

    // UI element references
    ListView m_AntDeviceListView;
    Label m_DeviceClassLabel;
    Label m_DeviceNameLabel;
    VisualElement m_DevicePortrait;

    public void InitializeAntDeviceList(VisualElement root, AntCollection antDevices)
    {
        devices = antDevices;
        devices.CollectionChanged += Devices_CollectionChanged;

        m_AntDeviceListView = root.Q<ListView>("device-list");

        // Store references to the selected character info elements
        m_DeviceClassLabel = root.Q<Label>("device-class");
        m_DeviceNameLabel = root.Q<Label>("device-name");
        m_DevicePortrait = root.Q<VisualElement>("device-portrait");

        m_AntDeviceListView.bindItem = (item, index) =>
        {
            Debug.Log($"bindItem: index = {index}");
            item.Q<Label>("device-name").text = m_AntDeviceListView.itemsSource[index].ToString();
        };

        m_AntDeviceListView.unbindItem = (item, index) =>
        {
            Debug.Log($"unbindItem: index = {index}");
        };
        m_AntDeviceListView.destroyItem = (item) => { Debug.Log($"destroyItem: item = {item}"); };

        m_AntDeviceListView.fixedItemHeight = 45;
        m_AntDeviceListView.itemsSource = new List<AntDevice>();
        m_AntDeviceListView.selectionChanged += OnDeviceSelected;
    }

    public void Update()
    {
        lock (deviceListLock)
        {
            if (updateList)
            {
                Debug.Log($"Update: update list - m_AntDeviceListView count = {m_AntDeviceListView.itemsSource.Count}");
                m_AntDeviceListView.RefreshItems();
                updateList = false;
            }
            if (clearDetails)
            {
                Debug.Log("Update: clear details");
                m_AntDeviceListView.ClearSelection();
                clearDetails = false;
            }
        }
    }

    private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        lock (deviceListLock)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    m_AntDeviceListView.itemsSource.Add((AntDevice)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    clearDetails = e.OldItems[0] == m_AntDeviceListView.selectedItem;
                    m_AntDeviceListView.itemsSource.Remove((AntDevice)e.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
            Debug.Log($"Devices_CollectionChanged: m_AntDeviceListView count = {m_AntDeviceListView.itemsSource.Count}, clearDetails = {clearDetails}");
            updateList = true;
        }
    }

    private void OnDeviceSelected(IEnumerable<object> enumerable)
    {
        AntDevice selectedDevice = (AntDevice)m_AntDeviceListView.selectedItem;
        Debug.Log($"OnDeviceSelected: selectedDevice = {selectedDevice}");

        // Handle none-selection (Escape to deselect everything)
        if (selectedDevice == null)
        {
            // Clear character details
            Debug.Log("OnDeviceSelected: clear details");
            m_DeviceClassLabel.text = "";
            m_DeviceNameLabel.text = "";
            m_DevicePortrait.style.backgroundImage = null;
        }
        else
        {
            // Fill in character details
            Debug.Log("OnDeviceSelected: fill details");
            m_DeviceClassLabel.text = selectedDevice.ChannelId.DeviceNumber.ToString();
            m_DeviceNameLabel.text = selectedDevice.ToString();
            m_DevicePortrait.style.backgroundImage = new StyleBackground(LoadImageFromAntDevice(selectedDevice));
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
