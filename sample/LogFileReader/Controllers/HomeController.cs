// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.PageReader;
using Castle.PageReader.Models;
using LogFileReader.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogFileReader.Controllers
{
    public class HomeController : Controller
    {
        public string LastLinesExample()
        {
            var lines = new List<string>();
            using (var reader = new System.IO.StreamReverseReader(@"Logs/logfile.txt"))
            {
                while (!reader.EndOfStream && lines.Count < 10)
                {
                    lines.Add(reader.ReadLine());
                }
            }
            return string.Join("\n", lines);
        }

        public string MiddleLinesExample()
        {
            var path = @"Logs/logfile.txt";
            var top = 0L;
            var bottom = 0L;
            var start = (new System.IO.FileInfo(path)).Length / 2;  // middle of file

            var lines = new List<string>();
            using (var reader = new System.IO.StreamReverseReader(path))
            {
                reader.Seek(start);         // set start position
                reader.ReadLine();          // read and discard (potentially) partial line
                top = reader.Position();    // position of first line in list
                while (!reader.EndOfStream && lines.Count < 10)
                {
                    lines.Add(reader.ReadLine());
                }
                bottom = reader.Position(); // position after last line in last
            }

            var result = $"Read position first line: {top}\n";
            result += $"Read position after last line: {bottom}\n";
            result += $"Number of lines read: {lines.Count()}\n";
            result += string.Join("\n", lines);
            return result;
        }

        public List<int> CountOptions = new List<int>() { 10, 25, 50, 100 };

        public async Task<ActionResult> Forward(string id)
        {
            return List(await PageReader.ReadFirstAsync(new PageReaderData()
            {
                Id = id,
                Count = CountOptions[0]
            }));
        }

        public async Task<ActionResult> ForwardFirst(PageReaderData logReader)
        {
            return List(await PageReader.ReadFirstAsync(logReader));
        }

        public async Task<ActionResult> ForwardLast(PageReaderData logReader)
        {
            return List(await PageReader.ReadLastAsync(logReader));
        }

        public async Task<ActionResult> ForwardNext(PageReaderData logReader)
        {
            return List(await PageReader.ReadNextAsync(logReader));
        }

        public async Task<ActionResult> ForwardPrev(PageReaderData logReader)
        {
            return List(await PageReader.ReadPrevAsync(logReader));
        }

        public async Task<ActionResult> Index()
        {
            var fileList = await PageReader.GetFilesAsync();
            var model = fileList.Select(file => new LogFileListModel(file));
            return View(model);
        }

        public async Task<ActionResult> Reverse(string id)
        {
            return List(await LogPageReader.ReadFirstAsync(new PageReaderData()
            {
                Id = id,
                Count = CountOptions[0]
            }));
        }

        public async Task<ActionResult> ReverseFirst(PageReaderData logReader)
        {
            return List(await LogPageReader.ReadFirstAsync(logReader));
        }

        public async Task<ActionResult> ReverseLast(PageReaderData logReader)
        {
            return List(await LogPageReader.ReadLastAsync(logReader));
        }

        public async Task<ActionResult> ReverseNext(PageReaderData logReader)
        {
            return List(await LogPageReader.ReadNextAsync(logReader));
        }

        public async Task<ActionResult> ReversePrev(PageReaderData logReader)
        {
            return List(await LogPageReader.ReadPrevAsync(logReader));
        }

        /// <summary>
        /// Performs operations for all routes to List view.
        /// </summary>
        public ActionResult List(PageReaderData page)
        {
            ModelState.Clear();
            if (page == null || string.IsNullOrWhiteSpace(page.Id))
                ModelState.AddModelError(string.Empty, "Bad or missing page read request.");
            ViewData["CountOptions"] = CountOptions;
            return View("List", new LogFileDataModel(page));
        }
    }
}