# LLRP 两层天线配置对比

## ReaderConfig 层

```
MSG_SET_READER_CONFIG
  └── AntennaConfiguration (PARAM_AntennaConfiguration[])
        └── [0]
              ├── AntennaID
              └── AirProtocolInventoryCommandSettings (UNION)
                    └── [0] PARAM_C1G2InventoryCommand
                          ├── TagInventoryStateAware
                          ├── C1G2Filter[]
                          ├── C1G2RFControl
                          │     ├── ModeIndex
                          │     └── Tari
                          └── C1G2SingulationControl
                                ├── Session (TwoBits)
                                ├── TagPopulation (ushort)
                                └── TagTransitTime (uint)
```

## ROSpec 层

```
MSG_ADD_ROSPEC
  └── ROSpec
        └── SpecParameter (PARAM_SpecParameter[])
              └── [0] PARAM_AISpec
                    ├── AntennaIDs (UInt16Array)
                    ├── AISpecStopTrigger
                    └── InventoryParameterSpec (PARAM_InventoryParameterSpec[])
                          └── [0]
                                ├── InventoryParameterSpecID
                                └── Protocol (UNION_AirProtocolInventoryCommandSettings)
                                      └── [0] PARAM_C1G2InventoryCommand
                                            ├── TagInventoryStateAware
                                            ├── C1G2Filter[]
                                            ├── C1G2RFControl
                                            │     ├── ModeIndex
                                            │     └── Tari
                                            └── C1G2SingulationControl
                                                  ├── Session (TwoBits)
                                                  ├── TagPopulation (ushort)
                                                  └── TagTransitTime (uint)
```

