using System;
using System.Collections;
using DB.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class BuildingsInteractionsMatrixTest
{
    private BuildingsInteractionsMatrix _matrix;

    private BuildingDefinition _farm;
    private BuildingDefinition _house;
    private BuildingDefinition _market;
    private BuildingDefinition _mine;
    
    [SetUp]
    public void LoadTestDataAsset()
    {
        _matrix = LoadAsset<BuildingsInteractionsMatrix>("TestData_BuildingsInteractionsMatrix.asset");
        
        _farm = LoadAsset<BuildingDefinition>("TestData_Farm.asset");
        _house = LoadAsset<BuildingDefinition>("TestData_House.asset");
        _market = LoadAsset<BuildingDefinition>("TestData_Market.asset");
        _mine = LoadAsset<BuildingDefinition>("TestData_Mine.asset");
    }
    
    [Test]
    public void TestGetValue_BuildingFarm()
    {
        Assert.AreEqual(1, _matrix.GetValue(_farm, _farm));
        Assert.AreEqual(1, _matrix.GetValue(_farm, _house));
        Assert.AreEqual(2, _matrix.GetValue(_farm, _market));
        Assert.AreEqual(0, _matrix.GetValue(_farm, _mine));
    }

    [Test]
    public void TestGetValue_BuildingHouse()
    {
        Assert.AreEqual(2, _matrix.GetValue(_house, _farm));
        Assert.AreEqual(1, _matrix.GetValue(_house, _house));
        Assert.AreEqual(1, _matrix.GetValue(_house, _market));
        Assert.AreEqual(-3, _matrix.GetValue(_house, _mine));
    }
    
    [Test]
    public void TestGetValue_BuildingMarket()
    {
        Assert.AreEqual(4, _matrix.GetValue(_market, _farm));
        Assert.AreEqual(2, _matrix.GetValue(_market, _house));
        Assert.AreEqual(0, _matrix.GetValue(_market, _market));
        Assert.AreEqual(2, _matrix.GetValue(_market, _mine));
    }
    
    [Test]
    public void TestGetValue_BuildingMine()
    {
        Assert.AreEqual(0, _matrix.GetValue(_mine, _farm));
        Assert.AreEqual(-5, _matrix.GetValue(_mine, _house));
        Assert.AreEqual(1, _matrix.GetValue(_mine, _market));
        Assert.AreEqual(-1, _matrix.GetValue(_mine, _mine));
    }
    
    private T LoadAsset<T>(string assetName) where T : Object
    {
        string path = "Assets/Tests/TestData/" + assetName;
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        Assert.IsNotNull(asset, "Could not load asset from " + path);
        return asset;
    }
}
