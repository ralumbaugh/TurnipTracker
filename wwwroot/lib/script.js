$(document).ready(function () {
    $("#accordion").accordion({
        collapsible: true,
        heightStyle: "content"
    });
    $(function() {
        $(".table").tablesorter();
    });
    $( function() {
        $( ".show-option" ).tooltip({
          show: {
            effect: "slideDown",
            delay: 250
          }
        });
        $( "#hide-option" ).tooltip({
          hide: {
            effect: "explode",
            delay: 250
          }
        });
        $( "#open-event" ).tooltip({
          show: null,
          position: {
            my: "left top",
            at: "left bottom"
          },
          open: function( event, ui ) {
            ui.tooltip.animate({ top: ui.tooltip.position().top + 10 }, "fast" );
          }
        });
      } );
    // $("#draggable").draggable();
})