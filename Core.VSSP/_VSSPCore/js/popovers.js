/**
 * --------------------------------------------------------------------------
 * CoreUI Free Boostrap Admin Template (v2.0.0): popovers.js
 * Licensed under MIT (https://coreui.io/license)
 * --------------------------------------------------------------------------
 */
$('[data-toggle="popover"]').popover();
$('.popover-dismiss').popover({
  trigger: 'focus'
});

$('[data-toggle="popover"][data-timeout]').on('shown.bs.popover', function () {
    console.log($(this).data("data-timeout"));
    this_popover = $(this);
    setTimeout(function () {
        this_popover.popover('hide');
    }, $(this).data("data-timeout"));
});

//# sourceMappingURL=popovers.js.map