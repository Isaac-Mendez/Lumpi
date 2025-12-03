
## **Gestión de usuarios (Registro e Inicio de Sesión)**

El sistema de Lumpi implementa un mecanismo completo de gestión de usuarios basado en una base de datos MySQL. La primera interacción real del usuario ocurre a través del formulario de registro, donde es necesario introducir datos obligatorios tales como nombre, apellidos, teléfono, correo y contraseña. Esta información pasa por un proceso de validación que asegura que no existan campos vacíos y que la estructura mínima de los datos sea la correcta.  
Una vez verificada la información, el sistema almacena los datos en la tabla correspondiente, garantizando que cada correo electrónico sea único dentro del sistema a través de validaciones SQL y manejo de excepciones.

El inicio de sesión utiliza el mismo conjunto de datos, donde el usuario introduce su correo y contraseña. La aplicación valida las credenciales mediante una consulta SQL parametrizada, y en caso de éxito, almacena el correo del usuario como referencia activa durante toda la sesión. Esta persistencia permite acceder a la información del perfil y asociar correctamente los hábitos creados a dicho usuario.


## **Creación de hábitos**

La creación de hábitos constituye el núcleo funcional de Lumpi. El sistema permite generar un nuevo hábito pulsando el botón “Añadir” dentro de la pantalla principal. El proceso se inicia con la creación de un prefab visual que representa el hábito, y simultáneamente se ejecuta la lógica de asignación de un identificador único generado a partir del nombre del usuario y un contador incremental.

Una vez asignado el ID, el sistema crea la estructura inicial del hábito: nombre por defecto, duración predeterminada de 60 minutos y estado no finalizado. Esta información se almacena inmediatamente en la base de datos, asegurando la persistencia del hábito aunque la aplicación se cierre.  
El hábito aparece inmediatamente en la scene, manteniendo la interfaz actualizada en tiempo real.


## **Edición de hábitos**

Cada hábito puede modificarse mediante un modo de edición accesible desde el panel visual. Al activar dicho modo, el usuario puede cambiar tanto el nombre como la duración original del hábito. Una vez confirmados los cambios, la aplicación recalcula la duración total en segundos, actualiza la visualización del tiempo y registra los cambios en la base de datos . Además, el sistema se encarga de reajustar el temporizador interno y actualizar la información guardada en PlayerPrefs, de modo que los datos editados se mantengan aunque el usuario abandone la aplicación, no obstante no esta funcionado del todo porque avances no se guarda el tiempo y el habito se reinicia.

## **Temporizador de hábitos**

Cada hábito contiene un temporizador independiente programado para funcionar en segundo plano y mantener el tiempo restante de manera persistente.  
Cuando el usuario pulsa el botón de inicio, el temporizador comienza a descender en intervalos de un segundo, actualizando en tiempo real la interfaz y guardando continuamente el tiempo restante en PlayerPrefs.

El sistema guarda automáticamente la fecha exacta de inicio en la base de datos, lo cual permite calcular el tiempo restante real aunque el usuario cierre la aplicación o el dispositivo entre en reposo.  
Si la cuenta atrás llega a cero, el sistema marca el hábito como finalizado y ejecuta la rutina que lo mueve a la tabla de hábitos finalizados.

## **Eliminación de hábitos**

Lumpi permite eliminar un hábito tanto desde la vista principal como durante su creación o edición.  
Al pulsar el botón correspondiente, el hábito es eliminado de la base de datos utilizando su identificador único y se elimina también de la interfaz. Esta operación solo afecta al hábito concreto del usuario logueado, garantizando así la consistencia de los datos.

## **Visualización del progreso**

El sistema incluye una scene donde el usuario puede observar de manera ordenada los hábitos que ha completado. Esta scene obtiene los datos desde la tabla _tabla_habitos_finalizados_, ordenándolos según la fecha más reciente.  
Cada hábito finalizado se muestra mediante un panel visual que representa el logro del usuario, facilitando el seguimiento de su progreso personal.

## **Gestión del perfil del usuario**

El usuario puede acceder a su información personal desde la scene de perfil. Esta sección obtiene directamente los datos desde la base de datos utilizando el correo almacenado en la sesión.  
Los datos se presentan como texto estático.

# **REQUISITOS NO FUNCIONALES**

## **Rendimiento del sistema**

El proyecto fue diseñado para ofrecer tiempos de respuesta mínimos, incluso en dispositivos Android de características limitadas.  
La conexión con la base de datos utilizada durante el desarrollo emplea consultas ligeras y optimizadas, evitando un uso excesivo del servidor. La carga de hábitos se realiza mediante una operación centralizada que recupera únicamente los datos necesarios, y el renderizado visual se actualiza de manera dinámica sin necesidad de recargar escenas completas.


## **Usabilidad y diseño**

La interfaz fue diseñada siguiendo principios de accesibilidad visual y simplicidad. Los colores elegidos, predominantemente morados oscuros, favorecen una atmósfera tranquila sin generar fatiga visual.  
Los botones son grandes, intuitivos y con iconos claros, lo cual facilita su uso incluso en pantallas pequeñas. El estilo caricaturesco añadido en algunos elementos gráficos contribuye a crear una aplicación visualmente amigable, adecuada para público joven o adulto.

## **Seguridad de la información**

La integridad de los datos del usuario se protege mediante el uso de consultas SQL parametrizadas que evitan inyecciones.  
El sistema prohíbe el registro de correos duplicados mediante validación directa de la base de datos y manejo interno de excepciones.  
Asimismo, el acceso a las pantallas internas de la aplicación está totalmente restringido para usuarios que no hayan pasado por el proceso de autenticación, evitando la exposición de datos privados.


## **Compatibilidad**

La aplicación fue diseñada para funcionar exclusivamente en dispositivos Android mediante exportación en formato APK.  
El uso del Canvas adaptativo de Unity permite ajustar automáticamente la interfaz a diferentes resoluciones, densidades de píxeles y orientaciones de pantalla, garantizando que todos los botones, paneles y textos se mantengan accesibles y visibles.

## **Mantenibilidad y estructura del código**

El proyecto está desarrollado siguiendo una arquitectura modular que separa elementos visuales, lógica del negocio y conexión con la base de datos.  
Scripts como _ConectorBD_, _ItemButton_ o _AddButtonSpawner_ cumplen responsabilidades únicas y bien definidas, facilitando la revisión y modificación futura del sistema.

La base de datos está centralizada en un único punto de conexión gestionado por el patrón Singleton, reduciendo errores de duplicación y simplificando tareas de mantenimiento.


# **BASE DE DATOS Y ESTRUCTURA E-R**

## **Estructura general de la base de datos**

La base de datos está diseñada como un sistema relacional compuesto por tres tablas principales:

- **registro_del_login** (usuarios)
    
- **tabla_habitos** (hábitos activos)
    
- **tabla_habitos_finalizados** (hábitos completados)
    

Los datos se encuentran relacionados mediante el campo **CorreoUsuario**, que actúa como enlace directo entre usuario y hábitos, estableciendo relaciones de uno a muchos.


## **Tablas y campos principales**

### **Tabla: registro_del_login**

Contiene la información del usuario:

- Nombre
    
- Apellidos
    
- Telefono
    
- Correo (clave primaria)
    
- Contraseña

### **Tabla: tabla_habitos**

Almacena hábitos activos:

- ID_Habito (clave primaria)
    
- CorreoUsuario (relación con usuario)
    
- Nombre
    
- DuracionMinutos
    
- Finalizado
    
- FechaRegistro
    
- FechaInicio

### **Tabla: tabla_habitos_finalizados**

Almacena hábitos completados:

- ID_Fin (PK)
    
- Nombre
    
- FechaFinalizacion
    
- CorreoUsuario

## **Modelo entidad-relación**

La relación principal es:

- **Un usuario puede tener múltiples hábitos activos**  
    (relación 1:N entre registro_del_login y tabla_habitos)
    
- **Un usuario puede tener múltiples hábitos finalizados**  
    (relación 1:N entre registro_del_login y tabla_habitos_finalizados)
    
- **Un hábito finalizado proviene siempre de un hábito activo previo**  
    (relación lógica 1:1 durante la migración)
    

Este modelo garantiza consistencia, integridad referencial y simplicidad en consultas.


# **CASOS DE USO**#

## **Flujo general del usuario**

El sistema propone un flujo secuencial muy claro:  
El usuario inicia en la pantalla de bienvenida, accede al login y, si no dispone de cuenta, procede a registrarse. Una vez autenticado, accede a la pantalla principal donde puede crear hábitos, editarlos, iniciarlos o eliminarlos. Tras finalizar hábitos, puede consultar su progreso en una pantalla dedicada y revisar sus datos personales en el perfil.

## **Casos de uso principales y detallados**

### **Iniciar sesión**

El usuario introduce su correo y contraseña.  
El sistema valida las credenciales mediante una consulta SQL y, si son correctas, carga los hábitos asociados y redirige a la pantalla principal.

### **Registrarse**

El usuario completa un formulario con sus datos.  
El sistema valida que los campos no estén vacíos y registra sus datos en la base de datos.

### **Crear un hábito**

Al pulsar “Añadir”, el sistema genera un ID único, crea un panel visual, registra el hábito en la BD y lo muestra en pantalla.

### **Editar un hábito**

Al activar el modo edición, el usuario cambia nombre y duración.  
El sistema actualiza tanto la interfaz como la base de datos.

### **Iniciar temporizador**

El usuario inicia la cuenta regresiva.  
El sistema guarda la fecha de inicio, actualiza el tiempo restante y gestiona la lógica interna hasta su finalización.

### **Eliminar un hábito**

El sistema elimina el hábito de la base de datos y lo retira de la interfaz.

### **Finalizar un hábito**

Cuando el temporizador llega a cero, el sistema migra el hábito a la tabla de finalizados y muestra una notificación visual.


# **DIAGRAMA DE CLASES**


## **Clases principales del sistema**

### **ConectorBD**

Gestiona toda la interacción con MySQL: login, registro, carga de hábitos, inserciones, actualizaciones y procesos internos. Es el núcleo del sistema, responsable del Singleton, de la sesión activa y del acceso global a los datos.

### **ItemButton**

Controla cada hábito individual: temporizador, edición, inicio, finalización y conexión con la base de datos. Es la clase con mayor lógica de negocio.

### **AddButtonSpawner**

Genera nuevos hábitos visuales y realiza el repintado de hábitos desde la base de datos.

### **PanelCelebracionUI**

Gestiona la representación visual de los hábitos completados.

### **PerfilManager**

Carga y muestra la información del usuario logueado.

### **Login**

Controla la entrada de credenciales y la comunicación con el ConectorBD para el inicio de sesión.


## **Relaciones entre clases**

- _ItemButton_ depende de _ConectorBD_ para guardar datos.
    
- _AddButtonSpawner_ depende de _ConectorBD_ para conocer hábitos previos.
    
- _PerfilManager_ usa _ConectorBD_ para obtener datos del usuario.
    
- _CelebracionSpawner_ consulta a _ConectorBD_ para obtener hábitos finalizados.
    
- _Login_ comunica los inputs de la escena hacia el Singleton _ConectorBD_.
    

La clase **ConectorBD** actúa como núcleo central que conecta todos los módulos del sistema.


# **LÓGICA DE NEGOCIO**

## **Lógica de creación de hábitos**

Cuando el usuario crea un hábito, el sistema genera un ID único basándose en el nombre del usuario y en un contador incrementado dinámicamente. La aplicación crea un objeto visual mediante un prefab, lo configura con valores por defecto y lo registra en la base de datos.  
Debido a que el registro ocurre de forma inmediata, el hábito queda disponible incluso después de cerrar la app.


## **Lógica del login**

El sistema recibe el correo y la contraseña desde los campos de la interfaz.  
Mediante una consulta SQL parametrizada verifica que exista un usuario con dichas credenciales. Si es así, guarda el correo del usuario, carga sus hábitos desde la base de datos y lo redirige a la pantalla principal.  
Todos los hábitos se cargan en memoria dentro de una lista estática para optimizar la experiencia.

## **Lógica de finalización de hábitos**

Cuando el temporizador alcanza el valor cero, el sistema:

1. Marca el hábito como finalizado.
    
2. Ejecuta un procedimiento almacenado que mueve el hábito a la tabla de finalizados.
    
3. Lo elimina de la tabla de hábitos activos.
    
4. Muestra una pantalla de celebración.
    
5. Limpia los datos almacenados en PlayerPrefs.

Este proceso asegura que el progreso del usuario quede registrado correctamente.


## **Validaciones internas**

La aplicación realiza validaciones como:

- Comprobación de campos vacíos durante el registro.
    
- Impedir duplicados mediante consultas SQL.
    
- Evitar iniciar temporizadores con valores inválidos.
    
- Evitar edición durante el funcionamiento del temporizador.
    
- Restringir el acceso a scene internas sin login.


# **FLUJO ENTRE SCENE**


## **Flujo principal**

El recorrido natural del usuario comienza en la scene de bienvenida, desde la cual puede acceder al login. Si no cuenta con un usuario registrado, la aplicación lo dirige al formulario de registro.  
Tras registrarse, accede nuevamente al login, donde al validar sus credenciales ingresa directamente en la pantalla de hábitos.  
Desde allí tiene acceso tanto al perfil como al progreso, siempre mediante rutas claras que permiten volver a la pantalla principal.

## **Conexión entre pantallas**

- Bienvenida → Login
    
- Login → Registro
    
- Registro → Login
    
- Login → Hábitos
    
- Hábitos → Perfil
    
- Perfil → Hábitos
    
- Hábitos → Progreso
    
- Progreso → Hábitos
    

Este flujo facilita una navegación intuitiva y una experiencia ordenada.
