// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Genel JavaScript işlevleri buraya eklenebilir

// Proje seçildiğinde kullanıcıları getir
function getProjeKullanicilari(projeId) {
    if (!projeId) {
        var kullaniciSelect = $('#AtananKullaniciId');
        kullaniciSelect.empty();
        kullaniciSelect.append($('<option></option>').val('').text('-- Kullanıcı Seçin --'));
        return;
    }

    $.ajax({
        url: '/Gorev/GetProjeKullanicilari',
        type: 'GET',
        data: { projeId: projeId },
        success: function (result) {
            var kullaniciSelect = $('#AtananKullaniciId');
            kullaniciSelect.empty();
            kullaniciSelect.append($('<option></option>').val('').text('-- Kullanıcı Seçin --'));
            
            if (result.success) {
                if (result.data && result.data.length > 0) {
                    $.each(result.data, function (index, item) {
                        kullaniciSelect.append($('<option></option>').val(item.id).text(item.email));
                    });
                } else {
                    alert('Bu projeye atanmış kullanıcı bulunmamaktadır.');
                }
            } else {
                alert('Kullanıcılar getirilirken bir hata oluştu: ' + result.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Hatası:', error);
            console.error('Status:', status);
            console.error('XHR:', xhr);
            alert('Bir hata oluştu: ' + error);
        }
    });
}
