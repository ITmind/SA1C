/*
 * Created by SharpDevelop.
 * User: Администратор
 * Date: 15.03.2011
 * Time: 21:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace Base1CGUI
{
	/// <summary>
	/// Description of Update.
	/// </summary>
	public static class Update
	{
		public static void CheckUpdate()
		{
			try{
				// Загружем страницу
				string data = GetHtmlPageText(@"http://sites.google.com/site/itmindco/sistema-avtomaticeskogo-obmena-dla-1s-8-h/nomer-versii" );
				
				// Тег для поиска, ищем теги <a></a>
				string tag1 = "b";
				string tag2 = "u";
				string pattern = string.Format( @"\<{0}.*?\>\<{1}.*?\>(?<tegData>.+?)\<\/{1}\>\<\/{0}\>", tag1.Trim(), tag2.Trim() );
				// \<{0}.*?\> - открывающий тег
				// \<\/{0}\> - закрывающий тег
				// (?<tegData>.+?) - содержимое тега, записываем в группу tegData
				
				Regex regex = new Regex( pattern, RegexOptions.ExplicitCapture );
				MatchCollection matches = regex.Matches( data );
				string ver = "3.5.1";
				
				foreach ( Match matche in matches ) {
					ver =matche.Groups[ "tegData" ].Value;
				}
				if(ver!="3.5.1"){
					MessageBox.Show("Доступна новая версия "+ver+" \n Проверьте страницчку программы.","Обновление SA1C",MessageBoxButton.OK,MessageBoxImage.Information);
				}
			}
			catch{
				
			}
		}
		
		public static string GetHtmlPageText( string url ) {
			WebClient client = new WebClient();
			using ( Stream data = client.OpenRead( url ) ) {
				using ( StreamReader reader = new StreamReader( data ) ) {
					return reader.ReadToEnd();
				}
			}
		}
	}
}
