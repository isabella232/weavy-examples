// restores any previous $ https://api.jquery.com/jquery.noconflict/
jQuery.noConflict();
if (jQuery.fn.jquery != $.fn.jquery) {
    console.log("Using jQuery: " + jQuery.fn.jquery + ", Global $: " + $.fn.jquery);
}
