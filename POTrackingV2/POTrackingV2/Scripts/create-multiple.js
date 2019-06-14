(function () {
  //$('#Engine').change(function(){
  //  if($(this).is(':checked')){    
  //    $('#Issue').prop('disabled',true)
  //    $('#None').prop('hidden',false)
  //    $('#Issue option').eq(1).prop('selected', true)
  //  }
  //  else{
  //    $('#Issue').prop('disabled',false)
  //    $('#None').prop('hidden',true)
  //    $('#Issue option').eq(0).prop('selected', true)
  //  }
  //})
  //var temp = 0;
  //var tempArray = [];
  //$("select").change(function () {
  //  var content = ($(this).val());
  //  let unique1 = tempArray.filter((o) => content.indexOf(o) === -1);
  //  let unique2 = content.filter((o) => tempArray.indexOf(o) === -1);
  //  const newItem = unique1.concat(unique2);
    
  //  //Add Row
  //  if(temp < content.length){
  //      $('table').append($("<tr id='"+newItem[0]+"'>")
  //          .append($("<th>").append(newItem[0]))
  //          .append($("<th>").append("<input type='checkbox'>"))
  //          .append($("<th>").append("<input type='checkbox'>"))
  //          .append($("<th>").append("<input type='checkbox'>"))
  //          );
  //          temp = temp +1;
  //          tempArray.push(newItem[0]);
  //          previous = this.value;
  //  }

  //  //Delete Row
  //  else{
  //      var deletededItem = tempArray.filter(function(obj) { return content.indexOf(obj) == -1; });
  //      document.getElementById(deletededItem).remove();
  //      tempArray = content;
  //      temp = temp -1;
  //  }
        
  //});

  $('.label.ui.dropdown')
  .dropdown();

  $('.no.label.ui.dropdown')
  .dropdown({
  useLabels: false
  });

  $('.ui.button').on('click', function () {
  $('.ui.dropdown')
    .dropdown('restore defaults')
  })
})();