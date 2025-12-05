
# *Diseño Previo del Proyecto*

El diseño de Lumpi comenzó con una planificación visual básica para definir cómo se distribuirían las scene, dónde irían los elementos interactivos y cuál sería la identidad estética. Antes de desarrollar la interfaz final en Unity, se elaboraron una serie de wireframes que sirvieron como guía visual y conceptual. Estos bocetos representaban la estructura de cada escena con colores básicos y elementos  gráficos poco detallados, centrándose únicamente en la organización.



Estos diseños previos permitieron visualizar el flujo lógico del usuario y prever la disposición de botones, inputs y paneles.



# **Diseño Final de la Interfaz**

Después del diseño conceptual y los wireframes, se construyó la interfaz definitiva en Unity. La aplicación mantiene una identidad visual coherente: colores suaves, estilo caricaturesco y elementos accesibles que permiten utilizar la app de manera cómoda.

Cada una de las pantallas principales de la aplicación quedó organizada de la siguiente forma:

## **1. Pantalla de Bienvenida**

_(Aquí va la imagen: unnamed.png)_  
Presenta el estilo visual, introduce al usuario en el entorno y ofrece el botón para avanzar al inicio de sesión.

## **2 Pantalla de Login**

_(Aquí va la imagen: unnamed (3).png)_  
Diseñada para ser simple y directa, con campos de correo y contraseña y un acceso al registro.

## **3 Pantalla de Registro**

_(Aquí va la imagen: unnamed (4).png)_  
Distribución vertical para mejorar la experiencia en dispositivos móviles, permitiendo completar el formulario de manera fluida.

## **4 Pantalla Principal de Hábitos**

_(Aquí va la imagen: unnamed (2).png)_  
Es el núcleo del sistema. Los hábitos se generan dinámicamente mediante prefabs, y cada uno cuenta con botones para editar, iniciar el temporizador o eliminarlo.

## **5 Pantalla de Perfil**

_(Aquí va la imagen: unnamed (1).png)_  
Carga automática desde la base de datos, mostrando los datos del usuario tal como se introdujeron en el registro.

## **6 Pantalla de Progreso**

_(Aquí colocar imagen: Pantalla_Progreso.png)_  
Aquí se visualizarán los hábitos completados y las celebraciones del progreso.


# **Diseño Técnico del Sistema**

El diseño técnico se centra en la estructura general del proyecto, tanto del lado de Unity como del lado de la base de datos y la organización del código.

## **Estructura del Proyecto en Unity**

Para mantener una arquitectura clara y ordenada, se creó una estructura de carpetas bien definida:

_(Aquí va la imagen: Estructura_Proyecto_Unity.png)_

- **Scenes**: cada pantalla principal.
    
- **Scripts**: todos los scripts del sistema.
    
- **Prefabs**: botones, paneles y elementos repetibles.
    
- **Sprites y UI**: recursos visuales.
    
- **TextMeshPro**: dependencias de Unity.
    

Esta organización permite incorporar nuevas funciones sin desordenar el proyecto.

---

## **3.2 Base de Datos – Diagrama E-R**

La base de datos se diseñó en MySQL utilizando un modelo relacional:

![[Pasted image 20251204130730.png]]


Incluye las siguientes tablas:

- **registro_del_login**
    
- **tabla_habitos**
    
- **tabla_habitos_finalizados**
    

Relaciones:

- Un usuario puede tener  0 o múltiples hábitos.
    
- Un hábito solo puede tener 1 como mínimo y 1 como máximo un usuario.
    
- Cada hábito finalizado solo puede tener 1 como mínimo y 1 como máximo habito original .



## **3.3 Diseño de Clases – UML**

La estructura del código se representa mediante un diagrama UML donde se muestran las clases principales y sus relaciones:

![[ChatGPT Image 4 dic 2025, 13_23_46.png]]

Las clases más importantes son:

- **ConectorBD** (núcleo del sistema)
    
- **ItemButton** (temporizador)
    
- **AddButtonSpawner** (creación dinámica de hábitos)
    
- **CelebracionSpawner** (paneles de celebración)
    
- **PerfilManager** (datos del usuario)
    
- **Login** (flujo de autenticación)
    

El UML debe mostrar dependencias, llamadas y referencias entre scripts.


# **Diseño de la Funcionalidad**

Esta sección explica la lógica de los scripts principales, pero integrándolos narrativamente en la explicación. No se presentan como listas, sino dentro del texto, señalando dónde insertar un fragmento de código.

## **ConectorBD – Diseño y Funcionamiento**

El script **ConectorBD.cs** es el componente central encargado de gestionar toda la comunicación con MySQL. Se utiliza un patrón **Singleton**, permitiendo que cualquier script pueda acceder a la base de datos sin inicializar conexiones repetidas.

El módulo gestiona:

- Registro de usuarios
    
- Login
    
- Consulta de datos
    
- Guardado de hábitos
    
- Migración de hábitos finalizados
    

Fragmentos importantes del código deben insertarse en los siguientes puntos:

- _(Aquí va bloque de código: ConectorBD_Login.png)_
    
- _(Aquí va bloque de código: ConectorBD_CargarHabitos.png)_
    
- _(Aquí va bloque de código: ConectorBD_Finalizacion.png)_
    

La narrativa explica que todas las funciones están optimizadas y separadas para evitar duplicación de lógica y garantizar estabilidad.


## **ItemButton – Diseño del Temporizador**

Este script controla el temporizador de cada hábito. Contiene la lógica para:

- Iniciar el conteo
    
- Detenerlo
    
- Editar duración
    
- Guardar el progreso
    
- Finalizar el hábito correctamente
    

La parte más importante del diseño es la **coroutine**, que permite contar el tiempo sin bloquear la aplicación. El fragmento de código más relevante debe insertarse aquí:

- _(Aquí va bloque de código: ItemButton_Timer.png)_
    

Además, el diseño permite edición fluida gracias a un modo editable que sincroniza los datos con la base de datos:

- _(Aquí va bloque de código: ItemButton_Edicion.png)_
    


## **Generación Dinámica – AddButtonSpawner**

El script AddButtonSpawner genera nuevos hábitos mediante prefabs. Desde el punto de vista del diseño, esto evita duplicación de UI y permite escalabilidad.

El código clave debe insertarse aquí:

- _(Aquí va bloque de código: Spawner_CrearHabito.png)_
    


## **Pantallas de Celebración**

La aplicación incluye un sistema visual para mostrar hábitos finalizados. El script CelebracionSpawner crea paneles automáticamente al cargar la escena.

Inserción del fragmento:

- _(Aquí va bloque de código: CelebracionSpawner_UI.png)_
    


## **4.5 PerfilManager – Diseño del Perfil**

La pantalla de perfil se alimenta directamente desde la base de datos. El diseño se centra en la claridad visual y el acceso rápido.

El fragmento del código relevante va aquí:

- _(Aquí va bloque de código: PerfilManager_ObtenerDatos.png)_

