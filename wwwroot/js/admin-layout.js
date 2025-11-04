(function ($) {
    "use strict";

    // 1. Lógica para el submenú colapsable (Gestor Clientes)
    $('.nav-item.has-treeview > .nav-link').on('click', function (e) {
        e.preventDefault(); 
        var $parentLi = $(this).parent('li');
        var $submenu = $parentLi.children('.nav-treeview');
        
        // Cierra otros menús abiertos
        $('.nav-item.has-treeview.menu-open').not($parentLi).removeClass('menu-open').children('.nav-treeview').slideUp(200);

        // Abre o cierra el menú actual
        $parentLi.toggleClass('menu-open');
        $submenu.slideUp(200, function () {
            if ($parentLi.hasClass('menu-open')) {
                $submenu.slideDown(200);
            }
        });
    });

    // 2. Lógica para el botón Hamburguesa (móvil)
    $('.sidebar-toggle-btn').on('click', function (e) {
        e.preventDefault();
        $('body').toggleClass('sidebar-open');
    });

    // 3. Lógica para el Overlay (cerrar sidebar al hacer clic fuera)
    $('#sidebar-overlay').on('click', function (e) {
        e.preventDefault();
        $('body').removeClass('sidebar-open');
    });

})(jQuery);