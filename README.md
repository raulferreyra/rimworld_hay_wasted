# Hay Waste Mod - RimWorld

## Descripción

Este mod de RimWorld implementa un sistema de subproductos de cosecha. Cuando los colonos cosechan plantas, generan automáticamente **Heno (Hay)** como un byproducto basado en la eficiencia de la cosecha.

### Mecánica

La cantidad de heno generada es **inversamente proporcional** al rendimiento de la cosecha:

- **Cosecha Fallida (0% rendimiento):** Genera 10 unidades de Hay
- **Cosecha Perfecta (100% rendimiento):** Genera 1 unidad de Hay
- **Fórmula:** `CantidadHeno = Clamp(11 - UnidadesCosechadas, 1, 10)`

### Características

- ✅ Compatible con RimWorld 1.4 y 1.5
- ✅ Utiliza Harmony para patches sin modificar archivos base
- ✅ Manejo robusto de errores y NullReferenceExceptions
- ✅ El Heno queda en el suelo (basura) hasta ser transportado o almacenado
- ✅ Totalmente modular y sin dependencias externas

## Instalación

1. Descarga el mod
2. Coloca la carpeta en: `RimWorld/Mods/`
3. Activa el mod en la pantalla de mods de RimWorld
4. Carga o crea una nueva partida

## Compilación (Desarrollo)

### Requisitos

- Visual Studio 2019+ o Rider
- .NET Framework 4.7.2
- RimWorld instalado localmente

### Pasos

1. Edita el archivo `Source/HayWasteMod.csproj`:

   ```xml
   <RimWorldInstallPath>C:\Program Files (x86)\Steam\steamapps\common\RimWorld</RimWorldInstallPath>
   ```

2. Abre el proyecto en Visual Studio y compila:

   ```bash
   dotnet build
   ```

3. El DLL compilado se copiará automáticamente a `Assemblies/HayWasteMod.dll`

## Archivos del Mod

```bash
hay_waste_mod/
├── About/
│   └── About.xml           # Metadatos del mod
├── Defs/
│   └── Plant_Hay.xml       # Definición del item Heno
├── Source/
│   ├── HarvestPatch.cs     # Lógica principal (Harmony patch)
│   └── HayWasteMod.csproj  # Archivo de proyecto C#
└── AGENT.md                # Documentación de especificaciones
```

## Estructura de Código

### HarvestPatch.cs

- **HarvestPatch**: Clase estática que inicializa Harmony
- **PlantHarvestPatch**: Patch que intercepta `JobDriver_PlantHarvest.MakeNewToils()`
- **SpawnHayByproduct()**: Método que genera el heno en el mundo

## Compatibilidad

- RimWorld 1.4+
- Harmony 2.2+
- No modifica archivos base (100% compatible)

## Autor

URAS - <https://urasweb.com>

## Licencia

Este mod es de propietario URAS. Ver licencia incluida.
